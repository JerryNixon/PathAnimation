# Extended Path Geometry
`ExtendedPathGeometry`, extends and provides some missing options for `PathGeometry` windows 10 UWP class.

#### Declaration / Instantiation
###### C#:
`ExtendedPathGeometry` provides a constructor with a `PathGeometry` parameter as also an extension method for `PathGeometry` type:
```cs
//constructor
public ExtendedPathGeometry(PathGeometry data);
//extension method
public static ExtendedPathGeometry ToExtendedPathGeometry(this PathGeometry geometry);
```
You can instantiate an `ExtendedPathGeometry` as follows:
```cs
var pg = new PathGeometry();
//instantiate using constructor
var p1 = new ExtendedPathGeometry(pg);
//instantiate using extension method
var p2 = pg.ToExtendedPathGeometry();
```
###### Xaml:
You cannot instantiate `ExtendedPathGeometry` directly in xaml


#### Properties

| Property | Description |
| :------- | :---------- |
| `PathGeometry` | Gets the original path geometry. |
| `PathOffset` | Contains information about potential blank space on the left and top of `PathGeometry`. This is usefull for croping path graphic. |
| `PathLength` | Gets total circumferential length of `ExtendedPathGeometry` |

#### Methods

###### GetPointAtFractionLength
*Gets the `Point` and a rotation theta on this `PathGeometry` at the specified fraction of its length.*
```cs
void GetPointAtFractionLength(double progress, out Point point, out double rotationTheta);
```
| Parameter | Description |
| :------- | :---------- |
| progress | Specified fraction in percent *(range [0,1])*. |
| point |  Contains the location at the specified progress value |
| rotationTheta | Contains the rotation *(in degrees [0,360])* at the specified progress value |

**Example usages:**

```cs
ExtendedPathGeometry pg = new ExtendedPathGeometry(pathGeometry);
Point p;
double rotationTheta;
//gets point and rotation theta at 20% of total length
pg.GetPointAtFractionLength(0.2, out p, out rotationTheta);
//gets point and rotation theta at fraction length = 100
pg.GetPointAtFractionLength(100.0 / pg.PathLength, out p, out rotationTheta);
```
