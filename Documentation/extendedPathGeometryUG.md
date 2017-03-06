# Extended Path Geometry
ExtendedPathGeometry, extends and provides some missing options for PathGeometry windows 10 UWP class.

#### Declaration / Instantiation
###### C#:
ExtendedPathGeometry provides a constructor with a PathGeometry parameter as also an extension method for PathGeometry types:
```cs
//constructor
public ExtendedPathGeometry(PathGeometry data);
//extension method
public static ExtendedPathGeometry ToExtendedPathGeometry(this PathGeometry geometry);
```
You can instantiate an ExtendedPathGeometry as follows:
```cs
var pg = new PathGeometry();
//instantiate using constructor
var p1 = new ExtendedPathGeometry(pg);
//instantiate using extension method
var p2 = pg.ToExtendedPathGeometry();
```
###### Xaml:
You cannot instantiate ExtendedPathGeometry directly in xaml


#### Properties

| Property | Description |
| :------- | :---------- |
| PathGeometry | Gets the original path geometry. |
| PathOffset | Contains information about potential blank space on the left and top of our path. This is usefull for croping path graphic. |
| PathLength | Gets total circumferential length of ExtendedPathGeometry |

#### Methods
| Method | Description |
| :------- | :---------- |
| GetPointAtFractionLength | Gets a point at fraction length. |