# Motion Path
A control used for moving an element by specifying coordinates or a path geometry.
![motionPath](images/motionPathExample.gif)

#### Declaration / Instantiation
In order to start using `MotionPath`, you must specify motion coordinates or the `Path` geometry where child will be translated.

'MotionPath' contains a default constructor, so you can declare it as follows:
###### C#:
```cs
MotionPath mp = new MotionPath();
mp.Content = new TextBlock() { Text = "A" };
mp.LineAbsoluteEnd = new Point(0, 0);
```
###### Xaml:
```xml
<controls:MotionPath LineAbsoluteEnd="0,0">
    <TextBlock Text="A" />
</controls:MotionPath>
```

**Important note:** All coordinates has a Point with `X = Y = double.NaN` by default. So, specifying `LineAbsoluteEnd=Point(0,0)`
makes MotionPath to move child to 0,0 position. 

If you wish to clear this point, set `LineAbsoluteEnd=Point(double.NaN,double.NaN)`

#### Properties

Properties of `MotionPath` control:

| Property | Description |
| :------- | :---------- |
| `AutoRewind` | Set to true if you wish your animation to auto rewind. |
| `CurrentPoint` | Gets the current point of element. |
| `CurrentTime` | Gets current animation time. |
| `Duration` | The duration of animation. |
| `EasingFunction` | The easing function of the animation. | 
| `LineRelativeEnd` | The animation line relative end point. |
| `LineAbsoluteStart` | The animation absolute start point. |
| `LineAbsoluteEndProperty` | The animation line absolute end point. Specifying this will make `MotionPath` to ignore `LineRelativeEnd` point. |
| `OrientToPath` | Set true if you want the child to rotate in order to follow animation orientation. |
| `Progress` | Current progress of animation |
| `Path` | Set `Path` if you wish your child to move over it. Setting `Path` will make `MotionPath` to ignore all specified points. |
| `State` | The current state of animation (`Ready`, `Running`, `Complete`, `Rewinding`, `Paused`) |

#### Methods
The following methods are available in `MotionPath`:

| Method | Description |
| :------- | :---------- |
| void Start() | Starts the animation. |
| async Task StartAsync() | Starts the animation asynchronously. |
| void Reset() | Resets current animation and child position. |
| void RewindNow() | Rewinds the animation when playing. |
| void Pause() | Pauses the animation. |

#### Events
The following events are available in `MotionPath`:

| Event | Description |
| :------- | :---------- |
| Starting | Cancelable event that occurs when state is ready and we are starting a new animation. |
| Started | Occurs when a new animation starts. |
| Completed | Occurs when an animation completes. |
| StateChanged | Occurs when animation state changes. |

#### FAQ
**ViewBox detected. Please specify control width and height.** <br/>
Because `ViewBox` causes problems when calculating point coordinates, when you want to have a `MotionPath` element inside a `ViewBox`, 
you have to explicitly specify MotionPath Width and Height values. This limitation does not apply when you are using a `Path` geometry.
