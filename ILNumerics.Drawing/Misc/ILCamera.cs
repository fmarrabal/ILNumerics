#region Copyright GPLv3
//
//  This file is part of ILNumerics.Net. 
//
//  ILNumerics.Net supports numeric application development for .NET 
//  Copyright (C) 2007, H. Kutschbach, http://ilnumerics.net 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
//  Non-free licenses are also available. Contact info@ilnumerics.net 
//  for details.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics; 

namespace ILNumerics.Drawing {
    /// <summary>
    /// This class represents the camera's positioning and aiming direction.
    /// </summary>
    [DebuggerDisplay("r:{m_distance} φ:{m_phiDebugDisp}° ρ:{m_rhoDebugDisp}° - P:{Position} - L:{LookAt}")]
    public class ILCamera {

        #region event handling 
        /// <summary>
        /// fires, if the position of the camera has changed
        /// </summary>
        public event EventHandler Changed;
        /// <summary>
        /// Fires a Changed event
        /// </summary>
        public virtual void OnChange() {
            if (Changed != null && !m_suspended)
                Changed(this,new EventArgs()); 
        }
        #endregion

        #region attributes 
        // input attributes (writable)
        private bool m_suspended = false;
        protected float m_posX;
        protected float m_posY;
        protected float m_posZ; 
        protected float m_topX;
        protected float m_topY;
        protected float m_topZ; 
        protected float m_lookAtX;
        protected float m_lookAtY;
        protected float m_lookAtZ;
        // output/cached attributes (readonly) 
        // polar coordinates 
        private float m_distance = 1.0f; 
        private float m_phi = 0; 
        private float m_rho = 0.0f;
        /// <summary>
        /// Offset angle for 2nd cached triangular phi value (needed for surface plots)
        /// </summary>
        internal float Offset = (float)(Math.PI / 4); 
        /// <summary>
        /// cachced triangular phi value with offset (needed for surface plots)
        /// </summary>
        internal float CosPhiShift;                
        /// <summary>
        /// cachced triangular phi value with offset (needed for surface plots)
        /// </summary>
        internal float SinPhiShift; 
        /// <summary>
        /// cached value for cosine of phi - this is readonly and for performance only.
        /// </summary>
        internal float CosPhi; 
        /// <summary>
        /// cached value for sine of phi - this is readonly and for performance only.
        /// </summary>
        internal float SinPhi; 
        /// <summary>
        /// cached value for cosine of rho - this is readonly and for performance only.
        /// </summary>
        internal float CosRho; 
        /// <summary>
        /// cached value for sine of rho - this is readonly and for performance only.
        /// </summary>
        internal float SinRho; 
        private CameraQuadrant m_quadrant;
        #endregion

        #region properties 

        /// <summary>
        /// point, the camera is aiming at (world coords)
        /// </summary>
        public ILPoint3Df LookAt {
            get { return new ILPoint3Df(m_lookAtX,m_lookAtY,m_lookAtZ); }
            set {
                //m_posX += (value.X - m_lookAtX);
                //m_posY += (value.Y - m_lookAtY);
                //m_posZ += (value.Z - m_lookAtZ); 
                m_lookAtX = value.X;
                m_lookAtY = value.Y;
                m_lookAtZ = value.Z;
                 
                updatePosition();
                OnChange();
            }
        }

        /// <summary>
        /// distance from the scene (readonly)
        /// </summary>
        public float Distance {
            get {
                return m_distance; 
            }
            set {
                m_distance = Math.Abs(value);
                updateCachedVars();
                updatePosition();
                OnChange(); 
            }
        }
        /// <summary>
        /// debugger helper: display phi in degrees (readonly)
        /// </summary>
        private int m_phiDebugDisp {
            get {
                return (int)Math.Round(m_phi * 180 / Math.PI); 
            }
        }
        /// <summary>
        /// debugger helper: display rho in degrees
        /// </summary>
        private int m_rhoDebugDisp {
            get {
                return (int)Math.Round(m_rho * 180 / Math.PI); 
            }
        }
        /// <summary>
        /// rotation of the scene (seen from above) [radians, readlony, rel. to lookat]
        /// </summary>
        public float Phi {
            get {
                return m_phi; 
            }
            set {
                m_phi = (float)((value + (Math.PI * 2)) % (Math.PI * 2));
                updateCachedVars();
                updatePosition();
                OnChange();
            }
        }
        /// <summary>
        /// pitch of the scene [radians], setting moves camera around lookat point
        /// </summary>
        public float Rho {
            get {
                return m_rho; 
            }
            set {
                if (value < 0.0f)
                    m_rho = 0.0f;
                else if (value > Math.PI)
                    m_rho = (float)Math.PI; 
                else 
                    m_rho = value; 
                updateCachedVars();
                updatePosition();
                OnChange(); 
            }
        }

        /// <summary>
        /// Quadrant the camera is currently watching the scene from
        /// </summary>
        public CameraQuadrant Quadrant {
            get{
                return m_quadrant; 
            }
        }
        /// <summary>
        /// Determine, if camera is placed in an upper quadrant of the scene
        /// </summary>
        public bool LooksFromTop {
            get {
                return m_rho < Math.PI/2; 
            }
        }
        /// <summary>
        /// Determine, if camera is located in an left quadrant of the scene
        /// </summary>
        public bool LooksFromLeft {
            get {
                return Math.Sin(m_phi) < 0; 
            }
        }
        /// <summary>
        /// Determine, if camera is located in an front quadrant of the scene
        /// </summary>
        public bool LooksFromFront {
            get {
                return Math.Cos(m_phi) >= 0; 
            }
        }
        /// <summary>
        /// true, when looking from top on the un-rotated scene (common for 2D plots)
        /// </summary>
        public bool Is2DView {
            get {
                return Math.Abs(SinPhi) < 1e-5 && Math.Abs(SinRho) < 1e-5; 
            }
        }
        /// <summary>
        /// get/set camera position, absolute cartesian coordinates
        /// </summary>
        /// <remarks>Keep in mind, the angle for phi points towards negative Y axis! The cartesian property 
        /// <paramref name="Position"/> handles the camera position in absolute world coordinates, while the 
        /// polar coordinates (Rho, Phi, Distance) supress the camera position by means of coordinates 
        /// relative to the LookAt point (i.e. usually the center of the viewing cube)!</remarks>
        public ILPoint3Df Position {
            get {
                ILPoint3Df ret = new ILPoint3Df(m_posX, m_posY, m_posZ);
                return ret; 
            }
            set {
                m_posX = value.X;
                m_posY = value.Y;
                m_posZ = value.Z; 
                cart2Pol(value - new ILPoint3Df(m_lookAtX, m_lookAtY, m_lookAtZ)); 
                updateCachedVars();
                OnChange(); 
            }
        }
        /// <summary>
        /// orientation of the camera, readonly
        /// </summary>
        /// <remarks>This vector is readonly always points 'upwards'.</remarks>
        public ILPoint3Df Top
        {
            get
            {
                ILPoint3Df ret = new ILPoint3Df(m_topX, m_topY, m_topZ);
                return ret;
            }
        }
        #endregion 

        #region constructors
        public ILCamera (ILCamera vport) {
            m_rho = vport.m_rho; 
            m_phi = vport.m_phi; 
            m_distance = vport.m_distance;
            m_lookAtX = vport.m_lookAtX;
            m_lookAtY = vport.m_lookAtY;
            m_lookAtZ = vport.m_lookAtZ;
            m_posX = vport.m_posX;
            m_posY = vport.m_posY;
            m_posZ = vport.m_posZ; 
            updateCachedVars();
            computeQuadrant();
        }
        /// <summary>
        /// Create a viewport: view at scene from top, no rotation
        /// </summary>
        public ILCamera () {
            m_posX = 0;
            m_posY = 0;
            m_posZ = 10; 
            m_quadrant = CameraQuadrant.TopLeftFront; 
            updateCachedVars(); 
        }
        public ILCamera (float Phi, float Rho, float Distance) {
            m_rho = Rho; 
            m_phi = Phi; 
            m_distance = Distance; 
            updateCachedVars();
            updatePosition(); 
            computeQuadrant();
        }
        #endregion

        #region public interface 
        /// <summary>
        /// suspend the firing of events until EventingResume() was called
        /// </summary>
        public void EventingSuspend() {
            m_suspended = true; 
        }
        /// <summary>
        /// Resume firing 'Change' events after it has been suspended
        /// </summary>
        public void EventingResume() {
            EventingResume(true); 
        }
        /// <summary>
        /// Resume firing 'Change' events, optionally skip pending events
        /// </summary>
        internal void EventingResume(bool fireEvents) {
            m_suspended = false;
            if (fireEvents) 
                OnChange(); 
        }
        /// <summary>
        /// Set both angles and distance at once  
        /// </summary>
        /// <param name="phi">Rotation, radians</param>
        /// <param name="rho">Pitch, radians</param>
        /// <param name="distance">Distance from scene</param>
        public void Set(float phi, float rho, float distance) {
            if (distance < 0) 
                throw new Exceptions.ILArgumentException("Camera distance must be positive!"); 
            m_phi = (float)(phi % (Math.PI * 2)); 
            m_rho = (float)(rho % (Math.PI)); 
            m_distance = distance;
            updateCachedVars();
            OnChange(); 
        }
        /// <summary>
        /// Set complete camera position (angles and distance) at once
        /// </summary>
        /// <param name="phi">Rotation (degrees)</param>
        /// <param name="rho">Pitch (degrees)</param>
        /// <param name="distance">Distance from scene</param>
        public void SetDeg(float phi,float rho, float distance) { 
            Set ((float)(phi/180.0 * Math.PI),(float)(rho / 180.0f * Math.PI),distance); 
        }
        /// <summary>
        /// Convert camera position to string
        /// </summary>
        /// <returns>string display with distance,roatation and pitch</returns>
        public override string ToString() {
            return String.Format("r:{0} φ:{1}° ρ:{2}° - P:{3} - L:{4}",
                m_distance,m_phiDebugDisp,m_rhoDebugDisp,Position,LookAt);
        }
        #endregion

        #region private helper 
        /// <summary>
        /// update internal cartesian (absolut) coordinates of position relative 
        /// to lookAt point. To be called after any polar coordinates were changed.
        /// </summary>
        private void updatePosition() {
            m_posX = m_lookAtX + (m_distance * SinRho * SinPhi);
            m_posY = m_lookAtY + (m_distance * SinRho * -CosPhi);
            m_posZ = m_lookAtZ + (m_distance * CosRho);
        }
        private void cart2Pol(ILPoint3Df cartVec) {
            cartVec.ToPolar(out m_distance, out m_phi, out m_rho);
        }
        ///// <summary>
        /////  update position, cons. polar coord and lookat as new
        ///// </summary>
        //private void pol2Cart() {
                
        //}

        private void computeQuadrant() {
            //if (m_phi == 0.0 && m_rho == 0.0) {
            //    m_quadrant = CameraQuadrant.TopLeftFront; 
            //    return; 
            //}
            if (m_rho < System.Math.PI / 2) {
                // top 
                if (m_phi < Math.PI) {
                    // right
                    if (m_phi < Math.PI / 2) {
                        // front
                        m_quadrant = CameraQuadrant.TopRightFront; 
                    } else {
                        // back
                        m_quadrant = CameraQuadrant.TopRightBack; 
                    }
                } else {
                    // left
                    if (m_phi > Math.PI / 2 * 3) {
                        // front
                        m_quadrant = CameraQuadrant.TopLeftFront;
                    } else {
                        // back
                        m_quadrant = CameraQuadrant.TopLeftBack;
                    }
                }
            } else {
                // bottom
                if (m_phi < Math.PI) {
                    // right
                    if (m_phi < Math.PI / 2) {
                        // front
                        m_quadrant = CameraQuadrant.BottomRightFront; 
                    } else {
                        // back
                        m_quadrant = CameraQuadrant.BottomRightBack; 
                    }
                } else {
                    // left
                    if (m_phi > Math.PI / 2 * 3) {
                        // front
                        m_quadrant = CameraQuadrant.BottomLeftFront; 
                    } else {
                        // back
                        m_quadrant = CameraQuadrant.BottomLeftBack; 
                    }
                }
            }
        }
        private void updateCachedVars () {
            CosPhi = (float)Math.Cos(m_phi); 
            SinPhi = (float)Math.Sin(m_phi);
            SinPhiShift = (float)Math.Sin(m_phi + Offset); 
            CosPhiShift = (float)Math.Cos(m_phi + Offset); 
            CosRho = (float)Math.Cos(m_rho); 
            SinRho = (float)Math.Sin(m_rho);
            // update top
            ILPoint3Df top = ILPoint3Df.normalize(-SinPhi * CosRho, CosPhi * CosRho, SinRho); 
            m_topX = top.X; 
            m_topY = top.Y;
            m_topZ = top.Z; 

            computeQuadrant();
        }
        #endregion
    }
}
