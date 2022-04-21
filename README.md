# CG-Animation-and-Transformations


an exercise in computer graphics course at HUJI 2021 about animation, homogeneous coordinates
and 3D transformations.

partner: amro asali

characterAnimator.cs draw and animate a 3D character on screen using BVH files 

the heirarchy section of the BVH files contains a hierarchical data structure (a tree in the files
included here) in which each node represents a joint in a skeleton ,
the segment between two joints is called a Bone,
joints with no children are called End Sites .

we iterate oveer the tree to build the skeleton by calcilating the final 3D position and adding the 
offset to the parents position all by using transformation matrices .
____________________________
the motion section contains list of keyframes each one representing the skeleton's pose at a 
point in time.
a keyframe is just an array of float values,
and each keayframe contains channels that correspond to the angles arouns specific axes of
specific rotations of specific joints 
the root joint has position channels .
this section also contais the number of frames in the animation and length of each frame in seconds.
at each frame we adjust the skeleton's pose according to keyframe channel data 

the global transformation for a given joint is the local transformation for it pre multiplied by
it's parents global tranformation
we transform each joint and animate the skeleton at each frame using the global transformation.


*the BVH format only allows for rigid transformations
