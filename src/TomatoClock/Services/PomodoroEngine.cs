using TomatoClock.Models;

namespace TomatoClock.Services;

public enum EngineState
{
    Idle,
    Running,
    Paused
}

/// <summary>
/// 番茄钟状态机。计时基于目标时间点(_phaseEndUtc)而非累加 tick,
/// 系统休眠/卡顿后时间自动补正,不失真。通过注入时钟便于单元测试。
/// </summary>
public class PomodoroEngine
{
    private readonly Func<AppSettings> _settings;
    private readonly Func<DateTime> _utcNow;

    private DateTime _phaseEndUtc;
    private TimeSpan _pausedRemaining;

    public EngineState State { get; private set; } = EngineState.Idle;
    public SessionType CurrentSession { get; private set; } = SessionType.Focus;

    /// <summary>当前周期内已完成的番茄数(长休息后归零)</summary>
    public int FocusInCycle { get; private set; }

    public TimeSpan Remaining { get; private set; }

    /// <summary>阶段自然完成(计入统计)</summary>
    public event EventHandler<SessionType>? PhaseCompleted;

    /// <summary>阶段被手动跳过(不计入统计)</summary>
    public event EventHandler<SessionType>? PhaseSkipped;

    public PomodoroEngine(Func<AppSettings> settings, Func<DateTime>? utcNow = null)
    {
        _settings = settings;
        _utcNow = utcNow ?? (static () => DateTime.UtcNow);
        Remaining = CurrentDuration;
    }

    public TimeSpan CurrentDuration => CurrentSession switch
    {
        SessionType.Focus => TimeSpan.FromMinutes(_settings().FocusMinutes),
        SessionType.ShortBreak => TimeSpan.FromMinutes(_settings().ShortBreakMinutes),
        SessionType.LongBreak => TimeSpan.FromMinutes(_settings().LongBreakMinutes),
        _ => TimeSpan.Zero
    };

    /// <summary>Idle 时开始当前阶段;Paused 时恢复</summary>
    public void Start()
    {
        switch (State)
        {
            case EngineState.Idle:
                BeginPhase();
                break;
            case EngineState.Paused:
                _phaseEndUtc = _utcNow() + _pausedRemaining;
                State = EngineState.Running;
                break;
        }
    }

    public void Pause()
    {
        if (State != EngineState.Running)
            return;

        _pausedRemaining = _phaseEndUtc - _utcNow();
        if (_pausedRemaining < TimeSpan.Zero)
            _pausedRemaining = TimeSpan.Zero;
        Remaining = _pausedRemaining;
        State = EngineState.Paused;
    }

    /// <summary>跳过当前阶段:立即进入下一阶段,不计入统计</summary>
    public void Skip()
    {
        var skipped = CurrentSession;
        var wasRunning = State == EngineState.Running;
        Advance();

        if (wasRunning)
            BeginPhase();
        else
            Remaining = CurrentDuration; // Idle/Paused:停留在新阶段的全量时长

        PhaseSkipped?.Invoke(this, skipped);
    }

    /// <summary>重置为初始状态(专注,未开始)</summary>
    public void Reset()
    {
        State = EngineState.Idle;
        CurrentSession = SessionType.Focus;
        FocusInCycle = 0;
        Remaining = CurrentDuration;
    }

    /// <summary>由外部定时器驱动;阶段结束时自动轮换</summary>
    public void Tick()
    {
        if (State != EngineState.Running)
            return;

        var remaining = _phaseEndUtc - _utcNow();
        if (remaining <= TimeSpan.Zero)
        {
            Remaining = TimeSpan.Zero;
            CompletePhase();
        }
        else
        {
            Remaining = remaining;
        }
    }

    private void CompletePhase()
    {
        var completed = CurrentSession;
        if (completed == SessionType.Focus)
            FocusInCycle++;

        PhaseCompleted?.Invoke(this, completed);
        Advance();

        if (_settings().AutoCycle)
        {
            BeginPhase();
        }
        else
        {
            State = EngineState.Idle;
            Remaining = CurrentDuration;
        }
    }

    private void BeginPhase()
    {
        var duration = CurrentDuration;
        _phaseEndUtc = _utcNow() + duration;
        Remaining = duration;
        State = EngineState.Running;
    }

    private void Advance()
    {
        if (CurrentSession == SessionType.Focus)
        {
            var interval = Math.Max(1, _settings().LongBreakInterval);
            if (FocusInCycle >= interval)
            {
                CurrentSession = SessionType.LongBreak;
                FocusInCycle = 0;
            }
            else
            {
                CurrentSession = SessionType.ShortBreak;
            }
        }
        else
        {
            CurrentSession = SessionType.Focus;
        }
    }
}
