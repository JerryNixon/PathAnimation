# Layout Path
A control used for placing / animating UIElements along a path.

It provides some usefull options such as rotation / translation smoothing, start / end behavior and more.

#### Declaration / Instantiation
In order to start using Layout Path, you must instantiate a class and specify the Path property.
Path is then analyzed and converted into an [ExtendedPathGeometry](extendedPathGeometryUG.md) for getting point and rotation at a fraction length. 
###### C#:
LayoutPath provides 2 constructors and an extension method for instantiating a new LayoutPath class.
```cs
//Default constructor
public LayoutPath();
//Overloaded constructor
public LayoutPath(PathGeometry pathGeometry);
//extension method
public static LayoutPath ToLayoutPath(this PathGeometry geometry);
```
So, you can create a new LayoutPath as follows:
```cs
PathGeometry yourPathGeometry = new PathGeometry();
//Instantiate by setting property
LayoutPath layoutPath1=new LayoutPath() { Path = yourPathGeometry };
//Instantiate by using contructor
LayoutPath layoutPath2 = new LayoutPath(pathGeometry: yourPathGeometry);
//instantiate by using path markup syntax
LayoutPath layoutPath3 = new LayoutPath(pathMarkup: "M 10,100 C 10,300 300,-200 300,100");
```

*Note: LayoutPath uses [StringToPathGeometryConverter](https://stringtopathgeometry.codeplex.com/SourceControl/latest#PathConverter/PathConverter/StringToPathGeometryConverter.cs)
codeplex project for converting path markup to path geometry and vice versa.*

###### Xaml:
You can declare a LayoutPath as follows:
```xml
<!-- initialize by using path markup -->
<controls:LayoutPath Path="M 10,100 C 10,300 300,-200 300,100">
	<!-- specify CacheMode="BitmapCache" to avoid flickering when animating text block -->
    <TextBlock Text="First child" CacheMode="BitmapCache"/>
    <TextBlock Text="Second child" />
</controls:LayoutPath>
<!--initialize by specifying Path geometry-->
<controls:LayoutPath>
    <controls:LayoutPath.Path>
        <PathGeometry>
            <PathFigure IsClosed="True" StartPoint="0,50">
                <LineSegment Point="150,0"/>
                <QuadraticBezierSegment Point1="280,60" Point2="400,0" />
                <ArcSegment Point="870,50" Size="9,3" SweepDirection="Clockwise"/>
            </PathFigure>
        </PathGeometry>
    </controls:LayoutPath.Path> 
    <TextBlock Text="First child" CacheMode="BitmapCache"/>    
    <Button Content="Second child" />
</controls:LayoutPath>
```