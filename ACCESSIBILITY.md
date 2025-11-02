# BulkIn Desktop - Accessibility Guide

## Overview

BulkIn Desktop is designed to be accessible to all users, including those using assistive technologies such as screen readers, keyboard-only navigation, and high-contrast themes.

## Keyboard Navigation

### Tab Navigation
- Press **Tab** to move forward through interactive elements
- Press **Shift+Tab** to move backward through interactive elements
- Press **Enter** or **Space** to activate buttons
- Press **Escape** to close dialogs and cancel operations

### Tab Order Priority
The application follows a logical tab order:
1. **Processing View**: Start → Pause → Stop buttons
2. **Settings View**: Input fields (top to bottom), then Test Connection → Save Settings → Reset buttons
3. **Logs View**: Search box → Filter toggles → Clear → Export buttons

## Screen Reader Support

### AutomationProperties
All interactive controls include:
- **Name**: Clear, descriptive label for the control
- **HelpText**: Detailed description of the control's purpose and any keyboard shortcuts

### Examples:
- **Start Processing Button**: "Start Processing. Begins processing all files in the queue. Keyboard shortcut: F5"
- **Settings Tab**: "Settings Tab. Configuration settings for database connection, file processing, and logging. Keyboard shortcut: Ctrl+2"

## High Contrast Theme Support

The application uses Avalonia's FluentTheme which automatically adapts to Windows High Contrast themes:
- All colors are defined using semantic names (Primary, Success, Warning, Error)
- Text colors provide sufficient contrast ratios (WCAG AA compliant)
- Focus indicators are clearly visible on all interactive elements

### Contrast Ratios:
- **Normal Text**: Minimum 4.5:1 ratio
- **Large Text (18pt+)**: Minimum 3:1 ratio
- **UI Components**: Minimum 3:1 ratio against background

## Color Considerations

While the application uses color to convey information, it never relies on color alone:
- **File Status**: Uses both colored icons AND status text (✅ Success, ❌ Failed, ⚙️ Processing)
- **Log Levels**: Uses icons in addition to colors (ℹ️ Info, ⚠️ Warning, ❌ Error)
- **Button States**: Uses both color and text labels

## Focus Management

### Visible Focus Indicators
All focusable elements display a clear focus indicator:
- Buttons: Border highlight when focused
- Text inputs: Border color change when focused
- Tab items: Background color change when focused

### Focus Behavior
- Focus automatically moves to the most relevant control after operations
- Modal dialogs trap focus until dismissed
- Focus returns to the triggering element when dialogs close

## Keyboard Shortcuts

All keyboard shortcuts are:
- **Non-conflicting**: Don't override system shortcuts
- **Documented**: Listed in tooltips and help documentation
- **Optional**: All features accessible via mouse/touch

See [KEYBOARD_SHORTCUTS.md](KEYBOARD_SHORTCUTS.md) for complete list.

## Text Scaling

The application supports Windows text scaling settings:
- Font sizes scale proportionally with system settings
- Layout adapts to accommodate larger text
- Minimum window size ensures content remains accessible at 200% scale

## Testing Recommendations

### For Users:
1. **Test with Narrator** (Windows built-in screen reader)
2. **Enable High Contrast theme** (Windows Settings → Ease of Access → High Contrast)
3. **Increase text size** (Windows Settings → Ease of Access → Display → Make text bigger)
4. **Navigate without mouse** (disconnect mouse and use keyboard only)

### For Developers:
1. Test with screen readers (NVDA, JAWS, Narrator)
2. Verify tab order is logical in all views
3. Check color contrast ratios with tools like [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
4. Test with Windows High Contrast themes
5. Verify keyboard shortcuts don't conflict with system shortcuts

## Accessibility Standards Compliance

BulkIn Desktop aims to meet:
- **WCAG 2.1 Level AA**: Web Content Accessibility Guidelines
- **Section 508**: U.S. federal accessibility requirements
- **Windows Accessibility Guidelines**: Microsoft's desktop application standards

## Known Limitations

1. **Screen Reader Support**: While basic screen reader support is implemented via AutomationProperties, full compatibility testing with all screen readers is ongoing
2. **Custom Controls**: Some custom-styled controls may require additional testing with assistive technologies

## Feedback

If you encounter accessibility issues or have suggestions for improvement, please:
1. Document the issue with steps to reproduce
2. Specify which assistive technology you're using
3. Include your Windows version and accessibility settings
4. Submit via your organization's support channel

## Resources

- [Microsoft Accessibility Guidelines](https://docs.microsoft.com/en-us/windows/apps/design/accessibility/accessibility)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Avalonia Accessibility Documentation](https://docs.avaloniaui.net/docs/concepts/accessibility)
- [Windows Narrator User Guide](https://support.microsoft.com/en-us/windows/complete-guide-to-narrator-e4397a0d-ef4f-b386-d8ae-c172f109bdb1)
