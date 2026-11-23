/// <summary>
/// 语义输入动作。全项目只有 InputSystem 认识物理按键，
/// 消费者只面对这个枚举，手柄/键盘映射改动不影响业务代码。
/// </summary>
public enum GameAction
{
    Interact,   // 交互
    Jump,       // 跳
    Sit,        // 坐
    Lie,        // 躺
    Crouch,     // 蹲伏（按住）
    Run,        // 奔跑（按住）
    Pause,      // 暂停/关闭面板
    Confirm,    // UI 确认
    Cancel,     // UI 取消
    DebugPanel, // 调试
}
