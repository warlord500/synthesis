﻿using System.Collections.Generic;
using System.Linq;
using Simulation_RD.SimulationPhysics;
using BulletSharp;
using OpenTK;

namespace Simulation_RD.Utility
{
    /// <summary>
    /// Extra Functions for manipulating the robot
    /// </summary>
    class AuxFunctions
    {
        /// <summary>
        /// Resets the robot to its starting position
        /// </summary>
        /// <param name="wheels"></param>
        /// <param name="parent"></param>
        public static void OrientRobot(List<BulletRigidNode> wheels, CollisionObject parent)
        {
            List<CollisionObject> rbs = (from w in wheels select w.BulletObject).Concat(new[] { parent }).ToList();
            rbs.ForEach(w => 
            {
                w.WorldTransform *= Matrix4.CreateTranslation(new Vector3(0, 75, 0) - w.WorldTransform.ExtractTranslation());
                w.InterpolationWorldTransform = Matrix4.Zero;
            });
        }        
    }
}
