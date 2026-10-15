# Time Scale On Toolbar

A sleek, space-efficient, and professional TimeScale utility for Unity Editor and Development Builds.

Designed specifically to **save horizontal space on the Unity Toolbar** while delivering maximum control: smart pause/resume, audio protection, mouse wheel adjustment, custom slider popup, and an in-game floating overlay for mobile/QA testing.

---

## ✨ Key Features

### 1. 🪶 Ultra-Compact Toolbar Design (~75px)
- **Zero Toolbar Clutter**: Takes only ~75px of horizontal space by default (`[⏸] [ 1.0x ▾ ]`), leaving plenty of room for Play buttons, scene tools, and other extensions.
- **Visual State Indicators**:
  - 🟠 **Amber/Orange**: Game is Paused (`0x`).
  - 🔷 **Cyan**: Game speed is modified (`!= 1.0x`).
  - Standard: Normal speed (`1.0x`).

### 2. ⏯️ Smart Pause & Resume
- Remembers your previous speed when pausing. Resuming restores the exact speed you were testing (e.g. 2.0x, 4.0x) instead of forcing you back to 1.0x.
- **Right-Click Pause Button**: Steps forward exactly **1 frame** (Frame Advance), essential for inspecting physics, VFX, and animation timing.

### 3. 🖱️ Fast Interactions (Zero Extra Buttons Needed)
- **Left-Click Pill**: Opens a dropdown menu with speed presets (`0.25x`, `0.5x`, `1.0x`, `2.0x`, `4.0x`, `8.0x`), Custom Slider, Frame Step, and Settings.
- **Scroll Wheel**: Hover your cursor over the speed pill and **scroll your mouse wheel** to quickly step speed up or down!
- **Double-Click or Middle-Click**: Instantly resets speed back to `1.0x`.

### 4. 🎚️ Custom Slider Popup
- Selecting **Custom Slider...** from the menu opens a sleek floating window with a slider (0x - 10x), numeric input field, and quick chips (`0.1x`, `0.5x`, `1x`, `2x`, `5x`, `10x`).

### 5. 🎧 Fast-Forward Audio Protection
- Automatically mutes or pauses `AudioListener` when `Time.timeScale > 1.0f` to prevent ear-piercing audio stutter during fast-forward playtesting. Automatically restores sound when returning to normal speed.

### 6. 📱 In-Game Mobile / QA Debugger (Runtime)
- Tested on Android, iOS, or Standalone builds? Thư mục `Runtime` cung cấp một floating badge nhỏ gọn tự động xuất hiện trong **Development Builds** (`#if DEVELOPMENT_BUILD || UNITY_EDITOR`).
- QA testers có thể kéo thả nút, chạm để mở menu điều chỉnh tốc độ game ngay trên thiết bị thực tế mà không cần nối cáp vào Unity Editor.
- Tự động loại bỏ hoàn toàn trong Release/Production builds.

---

## ⚙️ Configuration (Project Settings)

Mở **Edit > Project Settings > Ktgame > Time Scale Slider On Toolbar**:

| Setting | Default | Description |
| :--- | :--- | :--- |
| **Enable On Toolbar** | `true` | Bật/tắt hiển thị trên Unity Toolbar. |
| **Display Mode** | `Compact` | Chọn giữa `Compact` (~75px), `Preset Buttons Bar`, hoặc `Slider`. |
| **Toolbar Zone** | `Left` | Đặt ở bên trái (`Left`) hoặc bên phải (`Right`) của toolbar. |
| **Presets Array** | `[0, 0.25, 0.5, 1, 1.5, 2, 4, 8]` | Danh sách các mốc tốc độ nhanh. |
| **Enable Mouse Scroll** | `true` | Cuộn chuột trên nút tốc độ để tăng/giảm nhanh. |
| **Mute Audio On Fast-Forward** | `true` | Tự động tắt tiếng khi tốc độ > 1.0x. |
| **Auto-Reset On Exit Play** | `true` | Tự động trả về 1.0x khi dừng Play Mode. |
| **Enable In-Game Overlay** | `true` | Hiển thị floating HUD trong Development Builds. |

---

## 💻 C# Runtime API

Bạn có thể gọi trực tiếp API trong code gameplay nếu cần:

```csharp
using com.ktgame.utils.time_scale_on_toolbar;

// Đổi tốc độ an toàn
TimeScaleManager.SetTimeScale(2.0f);

// Tạm dừng và khôi phục (ghi nhớ tốc độ trước đó)
TimeScaleManager.Pause();
TimeScaleManager.Resume();
TimeScaleManager.TogglePause();

// Reset về 1.0x
TimeScaleManager.ResetTimeScale();

// Bắt sự kiện khi tốc độ thay đổi
TimeScaleManager.OnTimeScaleChanged += (newSpeed) => {
    Debug.Log($"Speed changed to {newSpeed}x");
};
```

---

## 📦 Compatibility
- Unity 2021.3, 2022.3, 2023.x, **Unity 6 (6000.x)**.
- Hỗ trợ đa nền tảng: Windows, macOS, Android, iOS.
