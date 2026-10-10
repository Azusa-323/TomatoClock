using TomatoClock.Models;
using TomatoClock.Services;

namespace TomatoClock.Tests;

public class PomodoroEngineTests
{
    private static (PomodoroEngine Engine, Action<TimeSpan> Advance) Create(AppSettings? settings = null)
    {
        settings ??= new AppSettings();
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var engine = new PomodoroEngine(() => settings, () => now);
        return (engine, span => now += span);
    }

    [Fact]
    public void Start_FromIdle_BeginsFocusPhase()
    {
        var (engine, _) = Create();

        engine.Start();

        Assert.Equal(EngineState.Running, engine.State);
        Assert.Equal(SessionType.Focus, engine.CurrentSession);
        Assert.Equal(TimeSpan.FromMinutes(25), engine.Remaining);
    }

    [Fact]
    public void Tick_BeforePhaseEnd_UpdatesRemaining()
    {
        var (engine, advance) = Create();
        engine.Start();

        advance(TimeSpan.FromMinutes(10));
        engine.Tick();

        Assert.Equal(TimeSpan.FromMinutes(15), engine.Remaining);
        Assert.Equal(SessionType.Focus, engine.CurrentSession);
    }

    [Fact]
    public void Tick_FocusElapsed_AutoAdvancesToShortBreakAndCounts()
    {
        var (engine, advance) = Create();
        var completed = new List<SessionType>();
        engine.PhaseCompleted += (_, type) => completed.Add(type);
        engine.Start();

        advance(TimeSpan.FromMinutes(26));
        engine.Tick();

        Assert.Contains(SessionType.Focus, completed);
        Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
        Assert.Equal(EngineState.Running, engine.State);
        Assert.Equal(TimeSpan.FromMinutes(5), engine.Remaining);
        Assert.Equal(1, engine.FocusInCycle);
    }

    [Fact]
    public void Tick_TimeJump_BeyondMultiplePhases_CompletesOnePhasePerTick()
    {
        // 休眠后时间大幅跳跃:一次 Tick 只完成一个阶段,不吞掉中间通知
        var (engine, advance) = Create();
        engine.Start();

        advance(TimeSpan.FromHours(3));
        engine.Tick();

        Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
        Assert.Equal(TimeSpan.FromMinutes(5), engine.Remaining);
    }

    [Fact]
    public void Pause_And_Resume_PreservesRemaining()
    {
        var (engine, advance) = Create();
        engine.Start();

        advance(TimeSpan.FromMinutes(10));
        engine.Pause();
        Assert.Equal(EngineState.Paused, engine.State);
        Assert.Equal(TimeSpan.FromMinutes(15), engine.Remaining);

        advance(TimeSpan.FromHours(2)); // 暂停期间时间流逝不应影响剩余
        engine.Start(); // 恢复
        engine.Tick();

        Assert.Equal(EngineState.Running, engine.State);
        Assert.Equal(TimeSpan.FromMinutes(15), engine.Remaining);
    }

    [Fact]
    public void CompleteFourFocusSessions_NextIsLongBreak_ThenCycleResets()
    {
        var (engine, advance) = Create();

        for (var i = 1; i <= 4; i++)
        {
            engine.Start();
            advance(TimeSpan.FromMinutes(26));
            engine.Tick(); // 专注完成

            if (i < 4)
            {
                Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
                advance(TimeSpan.FromMinutes(6));
                engine.Tick(); // 短休完成
                Assert.Equal(SessionType.Focus, engine.CurrentSession);
            }
        }

        Assert.Equal(SessionType.LongBreak, engine.CurrentSession);
        Assert.Equal(0, engine.FocusInCycle);

        advance(TimeSpan.FromMinutes(16));
        engine.Tick(); // 长休完成
        Assert.Equal(SessionType.Focus, engine.CurrentSession);
    }

    [Fact]
    public void AutoCycleOff_PhaseCompletes_StopsIdle()
    {
        var settings = new AppSettings { AutoCycle = false };
        var (engine, advance) = Create(settings);
        engine.Start();

        advance(TimeSpan.FromMinutes(26));
        engine.Tick();

        Assert.Equal(EngineState.Idle, engine.State);
        Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
        Assert.Equal(TimeSpan.FromMinutes(5), engine.Remaining);
    }

    [Fact]
    public void Skip_DoesNotCountFocus_AndAdvances()
    {
        var (engine, _) = Create();
        var skipped = new List<SessionType>();
        var completed = new List<SessionType>();
        engine.PhaseSkipped += (_, type) => skipped.Add(type);
        engine.PhaseCompleted += (_, type) => completed.Add(type);
        engine.Start();

        engine.Skip();

        Assert.Contains(SessionType.Focus, skipped);
        Assert.Empty(completed);
        Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
        Assert.Equal(0, engine.FocusInCycle);
        Assert.Equal(EngineState.Running, engine.State); // 跳过后继续运行新阶段
    }

    [Fact]
    public void Skip_WhilePaused_StaysPausedWithNewPhaseDuration()
    {
        var (engine, _) = Create();
        engine.Start();
        engine.Pause();

        engine.Skip();

        Assert.Equal(EngineState.Paused, engine.State);
        Assert.Equal(SessionType.ShortBreak, engine.CurrentSession);
        Assert.Equal(TimeSpan.FromMinutes(5), engine.Remaining);
    }

    [Fact]
    public void Reset_ReturnsToIdleFocus()
    {
        var (engine, advance) = Create();
        engine.Start();
        advance(TimeSpan.FromMinutes(26));
        engine.Tick(); // 进入短休
        advance(TimeSpan.FromMinutes(6));
        engine.Tick(); // 回到专注

        engine.Reset();

        Assert.Equal(EngineState.Idle, engine.State);
        Assert.Equal(SessionType.Focus, engine.CurrentSession);
        Assert.Equal(0, engine.FocusInCycle);
        Assert.Equal(TimeSpan.FromMinutes(25), engine.Remaining);
    }

    [Fact]
    public void PhaseSequence_FullCycle_MatchesExpectedPattern()
    {
        var (engine, advance) = Create();
        var sequence = new List<SessionType>();
        engine.PhaseCompleted += (_, type) => sequence.Add(type);

        // 完整跑完一个周期:4 专注 + 3 短休 + 1 长休
        foreach (var _ in Enumerable.Range(0, 8))
        {
            if (engine.State == EngineState.Idle)
                engine.Start();

            advance(TimeSpan.FromHours(2));
            engine.Tick();
        }

        var expected = new[]
        {
            SessionType.Focus, SessionType.ShortBreak,
            SessionType.Focus, SessionType.ShortBreak,
            SessionType.Focus, SessionType.ShortBreak,
            SessionType.Focus, SessionType.LongBreak
        };
        Assert.Equal(expected, sequence);
    }
}
