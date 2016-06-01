using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Oshmirto;


namespace Assets.scripts
{
    public class ShotFragmentInitFlexible : FireBoltAction
    {
        //passed in params
        private bool initialized = false;

        private string cameraName; //this is actually going to manipulate the rig most likely, but what we call it doesn't matter much from in here
        private CameraBody cameraBody; //need a reference to this guy for setting fstop and lens
        private string lensName;
        private string fStopName;
        private List<Framing> framings;
        private Oshmirto.Direction direction;
        private Oshmirto.Angle cameraAngle;
        private string focusTarget;

        //intermediate calculated values
        Vector3 targetLookAtPoint = new Vector3();

        //parameter grounding
        Vector3Nullable tempCameraPosition;
        Vector3Nullable tempCameraOrientation;
        ushort? tempLensIndex;
        ushort? tempFStopIndex;
        float? tempFocusDistance;

        //saved camera values
        GameObject camera;
        Quaternion previousCameraOrientation = Quaternion.identity;
        Vector3 previousCameraPosition = Vector3.zero;
        ushort previousLensIndex;
        ushort previousFStopIndex;
        float previousFocusDistance;

        //final camera values
        Quaternion newCameraOrientation;
        Vector3 newCameraPosition;
        ushort newLensIndex;
        ushort newFStopIndex;
        float newfocusDistance;

        public ShotFragmentInitFlexible(float startTick, string cameraName, string anchor, float? height, float? pan,
                                string lensName, string fStopName, List<Framing> framings, Oshmirto.Direction direction,
                                Oshmirto.Angle cameraAngle, string focusTarget) :
            base(startTick, startTick)
        {
            this.startTick = startTick;
            this.cameraName = cameraName;
            this.lensName = lensName;
            this.fStopName = fStopName;
            this.framings = framings;
            this.direction = direction;
            this.cameraAngle = cameraAngle;
            this.focusTarget = focusTarget;
        }

        public override bool Init()
        {
            if (initialized) return true;
            Extensions.RenderColliders();
            //don't throw null refs in the debug statement if framing isn't there.  it's not required
            string framingDescriptor = string.Empty;
            if (existsFraming())
                framingDescriptor = framings[0].ToString();

            Extensions.Log("init shot fragment start[{0}] end[{1}] lens[{2}] fStop[{3}] framing[{4}] direction[{5}] angle[{6}] focus[{7}]",
                                    startTick, endTick, lensName, fStopName, framingDescriptor, direction, cameraAngle, focusTarget);

            if (!findCamera()) return false;
            savePreviousCameraState();

            //ground parameters
            tempCameraPosition = new Vector3Nullable(null, null, null);
            tempCameraOrientation = new Vector3Nullable(null, null, null);

            //subject position if something is framed            
            GameObject framingTarget = null;
            if (existsFraming())
            {
                if (getActorByName(framings[0].FramingTarget, out framingTarget))
                {
                    targetLookAtPoint = findTargetLookAtPoint(framingTarget);
                }
                else
                {
                    Debug.LogError(string.Format("could not find actor [{0}]",
                    framings[0].FramingTarget).AppendTimestamps());
                }
            }

            //height
            if (existsFraming()) //default to even height with point of interest on framed target
            {
                tempCameraPosition.Y = targetLookAtPoint.y;
            }
            else
            {
                tempCameraPosition.Y = 1; //in the absence of all information just put the camera not at 0 height
            }

            //lens 
            ushort tempLens;
            if (!string.IsNullOrEmpty(lensName) &&
               CameraActionFactory.lenses.TryGetValue(lensName, out tempLens))
            {
                tempLensIndex = tempLens;
            }

            //F Stop
            ushort tempFStop;
            if (!string.IsNullOrEmpty(fStopName) &&
               CameraActionFactory.fStops.TryGetValue(fStopName, out tempFStop))
            {
                tempFStopIndex = tempFStop;
            }

            //framing 
            if (existsFraming() && framingTarget)
            {
                Bounds targetBounds = framingTarget.GetComponent<BoxCollider>().bounds;
                targetBounds.BuildDebugBox(5, Color.cyan);

                Extensions.Log("framing target[{0}] bounds[{1},{2}]", framingTarget.name,
                                        targetBounds.min.y, targetBounds.max.y);

                FramingParameters framingParameters = FramingParameters.FramingTable[framings[0].FramingType];

                //default our aperture to one appropriate to the framing if it's not set
                if (!tempFStopIndex.HasValue &&
                    CameraActionFactory.fStops.TryGetValue(framingParameters.DefaultFStop, out tempFStop))
                    tempFStopIndex = tempFStop;

                if (tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)
                {
                    //case is here for completeness.  rotation needs to be done for all combinations of lens and anchor specification, so it goes after all the conditionals
                }
                else if (!tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)//direction still doesn't matter since we can't move in the x,z plane
                {
                    //naively guessing and checking
                    Quaternion savedCameraRotation = cameraBody.NodalCamera.transform.rotation;
                    //point the camera at the thing
                    cameraBody.NodalCamera.transform.rotation = Quaternion.LookRotation(targetBounds.center - cameraBody.NodalCamera.transform.position);
                    float targetFov = 0;
                    //need to keep from stepping up and down over some boundary
                    bool incremented = false;
                    bool decremented = false;
                    while (!(incremented && decremented))  //if we haven't set a value and we haven't stepped both up and down.  
                    {
                        //find where on the screen the extents are.  using viewport space so this will be in %. z is world units away from camera
                        Vector3 bMax = cameraBody.NodalCamera.WorldToViewportPoint(targetBounds.max);
                        Vector3 bMin = cameraBody.NodalCamera.WorldToViewportPoint(targetBounds.min);

                        float FovStepSize = 2.5f;//consider making step size a function of current size to increase granularity at low fov.  2.5 is big enough to jump 100-180 oddly

                        if (bMax.y - bMin.y > framingParameters.MaxPercent && bMax.y - bMin.y < framingParameters.MinPercent)
                        {
                            break;//we found our answer in cameraBody.NodalCamera.fov
                        }
                        else if (bMax.y - bMin.y < framingParameters.MinPercent)
                        {
                            cameraBody.NodalCamera.fieldOfView -= FovStepSize;
                            decremented = true;
                        }
                        else //(bMax.y - bMin.y >= fp.MaxPercent)
                        {
                            cameraBody.NodalCamera.fieldOfView += FovStepSize;
                            incremented = true;
                        }

                        //force matrix recalculations on the camera after adjusting fov
                        cameraBody.NodalCamera.ResetProjectionMatrix();
                        cameraBody.NodalCamera.ResetWorldToCameraMatrix();
                    }
                    //reset camera position...we should only be moving the rig
                    targetFov = cameraBody.NodalCamera.fieldOfView;
                    cameraBody.NodalCamera.transform.rotation = savedCameraRotation;
                    tempLensIndex = (ushort)ElPresidente.Instance.GetLensIndex(targetFov);
                }
                else if (tempLensIndex.HasValue && //direction matters here.  
                    (!tempCameraPosition.X.HasValue || !tempCameraPosition.Z.HasValue))//also assuming we get x,z in a pair.  if only one is provided, it is invalid and will be ignored
                {
                    var cpl = new CameraPositionAndLens();
                    if (!findCameraPositionByRadius(framingTarget, targetBounds, framingParameters, tempLensIndex.Value, 0.5f, out cpl))
                    {
                        Extensions.Log("failed to find satisficing position for camera to frame [{0}] [{1}] with lens [{2}]. using least bad alternative",
                                                framings[0].FramingTarget, framings[0].FramingType.ToString(), ElPresidente.Instance.lensFovData[tempLensIndex.Value]._focalLength);
                    }
                    tempCameraPosition.X = cpl.Position.x;
                    tempCameraPosition.Y = cpl.Position.y;
                    tempCameraPosition.Z = cpl.Position.z;
                }
                else //we are calculating everything by framing and direction.  
                {
                    var cpl = new CameraPositionAndLens();
                    var bestCPL = new CameraPositionAndLens() { Badness = float.MaxValue };
                    ushort defaultLensIndex = CameraActionFactory.lenses[framingParameters.DefaultFocalLength];
                    ushort bestLensIndex = defaultLensIndex;
                    tempLensIndex = defaultLensIndex;
                    const float BADNESS_WEIGHT_LENS_CHANGE = 0.75f;
                    const float BADNESS_THRESHOLD = 3.1f;
                    //x,z does not have value
                    //pick a typical lens for this type of shot
                    //ushort defaultLensIndex = CameraActionFactory.lenses[framingParameters.DefaultFocalLength];
                    //tempLensIndex = defaultLensIndex;

                    //see if we can find a camera location for this lens
                    //allow less than some % of a circle variance from ideal viewing.  if we don't find an answer, change the lens

                    bool sign = true;
                    short iterations = 0;
                    ushort maxLensChangeIterations = 6;
                    while (!findCameraPositionByRadius(framingTarget, targetBounds, framingParameters, tempLensIndex.Value, 0.35f, out cpl))
                    {
                        //add lens change badness to result of camera position find
                        cpl.Badness += BADNESS_WEIGHT_LENS_CHANGE * Mathf.Abs(tempLensIndex.Value - defaultLensIndex);
                        if (cpl.Badness < bestCPL.Badness)
                        {
                            bestCPL = cpl;
                            bestLensIndex = tempLensIndex.Value;
                            if (bestCPL.Badness < BADNESS_THRESHOLD)
                            {
                                break;
                            }
                        }

                        iterations++;
                        if (iterations > maxLensChangeIterations)
                        {
                            Extensions.Log("exceeded max lens change iterations[{0}] solving framing[{1}] on target[{2}]",
                                                    maxLensChangeIterations, framingParameters.Type, framingTarget);
                            break; //framing is just not working out.  we will return a shot that's not so good and get on with things
                        }
                        int offset = sign ? iterations : -iterations;
                        if (tempLensIndex + offset < 0)
                        {
                            //should never get here since the smallest we specify is 27mm and we will cap at +-3 lenses
                        }
                        else if (tempLensIndex + offset > CameraActionFactory.lenses.Values.Max<ushort>())
                        {
                            //explore on the other side of our start lens until we hit our max iterations
                            iterations++;
                            offset = sign ? -iterations : iterations;
                        }
                        tempLensIndex = (ushort)(tempLensIndex + offset);
                    }
                    tempCameraPosition.X = bestCPL.Position.x;
                    tempCameraPosition.Y = bestCPL.Position.y;
                    tempCameraPosition.Z = bestCPL.Position.z;
                    tempLensIndex = bestLensIndex;
                }
                tempCameraOrientation.Y = Quaternion.LookRotation(framingTarget.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.y;
            }

            //this destroys the ability to angle with respect to anything but the framing target if specified
            //this does not seem terribly harmful. subject is attached to angle mostly because we wanted to not have
            //to specify a framing (if we used an absolute anchor for camera positioning)
            tiltCameraAtSubject(cameraAngle, framingTarget);

            //focus has to go after all possible x,y,z settings to get the correct distance to subject
            Vector3 focusPosition;
            if (calculateFocusPosition(focusTarget, out focusPosition))
            {
                tempFocusDistance = Vector3.Distance(tempCameraPosition.Merge(previousCameraPosition), focusPosition);
            }
            else if (framingTarget != null)//we didn't specify what to focus on, but we framed something.  let's focus on that by default
            {
                tempFocusDistance = Vector3.Distance(tempCameraPosition.Merge(previousCameraPosition), targetLookAtPoint);
            }

            //sort out what wins where and assign to final camera properties
            //start with previous camera properties in case nothing fills them in
            newCameraPosition = tempCameraPosition.Merge(previousCameraPosition);
            newCameraOrientation = Quaternion.Euler(tempCameraOrientation.Merge(previousCameraOrientation.eulerAngles));
            newLensIndex = tempLensIndex.HasValue ? tempLensIndex.Value : previousLensIndex;
            newFStopIndex = tempFStopIndex.HasValue ? tempFStopIndex.Value : previousFStopIndex;
            newfocusDistance = tempFocusDistance.HasValue ? tempFocusDistance.Value : previousFocusDistance;

            Skip();

            initialized = true;
            return initialized;
        }

        private void tiltCameraAtSubject(Angle cameraAngle, GameObject framingTarget)
        {
            //if there is a framing target, then we can use the lookat point on it
            if (framingTarget)
            {
                tempCameraOrientation.X = Quaternion.LookRotation(targetLookAtPoint -
                                          tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.x;
                return;
            }

            //if no framing target, we try to find the angle target
            GameObject subject = framingTarget;
            if (!subject &&
                !getActorByName(cameraAngle.Target, out subject))
            {
                Extensions.Log("Cannot find subject to as framing target or angle target to tilt toward");
                return;
            }
            tempCameraOrientation.X = Quaternion.LookRotation(subject.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.x;
        }

        private bool existsFraming()
        {
            return framings.Count > 0 && framings[0] != null;
        }

        private Vector3 findTargetLookAtPoint(GameObject target)
        {
            CinematicModel.Actor actor;
            ElPresidente.Instance.CinematicModel.TryGetActor(target.name, out actor); //find the CM definition for the actor we are supposed to angle against
            Framing framing = framings.Find(x => x.FramingTarget == target.name); //see if there is a target being framed with that name

            float pointOfInterestScalar = 0;
            if (framing != null && framing.FramingType <= FramingType.Waist)
                pointOfInterestScalar = actor.PointOfInterest;

            Vector3 targetLookAtPoint = new Vector3();
            var collider = target.GetComponent<BoxCollider>();
            if (collider != null)
            {
                targetLookAtPoint = new Vector3(collider.bounds.center.x,
                                            collider.bounds.center.y + pointOfInterestScalar * collider.bounds.extents.y,
                                            collider.bounds.center.z);
            }
            else
            {
                targetLookAtPoint = target.transform.position;
                Extensions.Log("actor [{0}] has no collider to calculate look-at point.  using actor root",
                                        target.name);
            }
            return targetLookAtPoint;
        }

        struct DistanceSet
        {
            public float min, max, ideal;

            public static DistanceSet GetDistanceToCamera(ushort lensIndex, Bounds targetBounds, FramingParameters framingParameters)
            {
                var distanceSet = new DistanceSet();
                float frustumHeight;
                //find vertical field of view for given lens
                float vFov = ElPresidente.Instance.lensFovData[lensIndex]._unityVFOV * Mathf.Deg2Rad;

                frustumHeight = getFrustumHeight(framingParameters.MinPercent, targetBounds);
                distanceSet.min = getDistanceForHeight(frustumHeight, vFov);

                frustumHeight = getFrustumHeight(framingParameters.MaxPercent, targetBounds);
                distanceSet.max = getDistanceForHeight(frustumHeight, vFov);

                frustumHeight = getFrustumHeight(framingParameters.TargetPercent, targetBounds);
                distanceSet.ideal = getDistanceForHeight(frustumHeight, vFov);

                return distanceSet;
            }

            /// <summary>
            /// find required height of view frustum at target
            /// </summary>
            private static float getFrustumHeight(float percentHeight, Bounds targetBounds)
            {
                return (1.0f / percentHeight) * (targetBounds.max.y - targetBounds.min.y);
            }

            /// <summary>
            /// find distance required to achieve frustum height based on vertical field of view
            /// </summary>
            private static float getDistanceForHeight(float frustumHeight, float vFov)
            {
                return frustumHeight / Mathf.Tan(vFov / 2.0f);
            }
        }

        private Opacity getTreeOpacity(Vector3 hitPoint)
        {
            Opacity opacity = Opacity.None;
            float minx = 0;
            float minz =0;
            float scale = 0.51f;
            foreach (var tree in Terrain.activeTerrain.terrainData.treeInstances)
            {
                var prototype = Terrain.activeTerrain.terrainData.treePrototypes[tree.prototypeIndex];
                var boxCollider = prototype.prefab.GetComponent<BoxCollider>();
                Vector3 treePosition = tree.position;
                treePosition.Scale(Terrain.activeTerrain.terrainData.size);
                treePosition += Terrain.activeTerrain.transform.position;
                if (!boxCollider) break;
                if (minx > treePosition.x + boxCollider.center.x - boxCollider.size.x) minx = treePosition.x + boxCollider.center.x - boxCollider.size.x;
                if (minz > treePosition.z + boxCollider.center.z - boxCollider.size.z) minz = treePosition.z + boxCollider.center.z - boxCollider.size.z;
                if (hitPoint.x > treePosition.x + boxCollider.center.x + boxCollider.size.x * scale ||
                    hitPoint.x < treePosition.x + boxCollider.center.x - boxCollider.size.x * scale ||
                    hitPoint.z > treePosition.z + boxCollider.center.z + boxCollider.size.z * scale ||
                    hitPoint.z < treePosition.z + boxCollider.center.z - boxCollider.size.z * scale)
                {
                    continue;
                }
                else
                {
                    opacity = prototype.prefab.GetComponent<OcclusionDescriptor>().colliderOpacity;
                    break;
                }                
            }
            return opacity;
        }

        private bool findCameraPositionByRadius(GameObject framingTarget, Bounds targetBounds, FramingParameters framingParameters,
                                                ushort lensIndex, float maxHorizontalSearchPercent, out CameraPositionAndLens result)
        {
            //if the badness is low, get on with life
            float BADNESS_THRESHOLD = 0.15f;

            result = new CameraPositionAndLens()
            {
                LensIndex = lensIndex,
                Badness = float.MaxValue,
                Position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)
            };

            bool subjectVisible = false;

            //find min,max,ideal camera distance for this lens
            var distanceToCamera = DistanceSet.GetDistanceToCamera(lensIndex, targetBounds, framingParameters);

            //search and track best match           

            float horizontalSearchSign = 1;
            float horizontalSearchStepSize = 5f * Mathf.Deg2Rad;
            ushort horizontalSearchIterations = 0;
            float horizontalSearchAngleCurrent = 0;
            //bool verticalSearchSign = true; only searching up atm
            float verticalSearchStepSize = 1.5f * Mathf.Deg2Rad;
            ushort verticalSearchIterations = 0;
            ushort verticalSearchIterationsMax = 10;
            float verticalSearchAngleInitial = (cameraAngle == null ? 0f : CameraActionFactory.angles[cameraAngle.AngleSetting]) * Mathf.Deg2Rad;

            RaycastHit hit;
            while (!subjectVisible)//search over the range about ideal position
            {
                horizontalSearchIterations++;

                //reset vertical angle to default/specified
                float verticalSearchAngleCurrent = verticalSearchAngleInitial;
                verticalSearchIterations = 0;
                while (!subjectVisible && verticalSearchIterations < verticalSearchIterationsMax)
                {
                    //find normalized direction vector given "direction" and angle measure
                    float subjectToCameraHeading = getBaseCameraHeading(direction, framingTarget);
                    Vector3 subjectToCamera = get3DDirection(framingTarget,
                                                             new Vector3(verticalSearchAngleCurrent, horizontalSearchAngleCurrent),
                                                             subjectToCameraHeading);

                    //raycast to check for LoS                    
                    Debug.DrawRay(targetLookAtPoint, subjectToCamera, Color.magenta, 10);
                    if (Physics.Raycast(targetLookAtPoint, subjectToCamera, out hit))
                    {
                        Opacity opacity = Opacity.High;
                        if (hit.collider.gameObject.name == "Terrain" && hit.point.y > 0.26f) //below a certain height, we expect ground.  this only works while we have flatness
                        {
                            opacity = getTreeOpacity(hit.point);                            
                        }

                        //if we get a hit, we will have to put the camera between the hit object and the
                        //subject.  again we are ignoring the range of valid sizes.  we should use
                        //min/max as stdev guidelines for "goodness" distribution

                        //find distance to hit point from subject 
                        var distToHit = (targetLookAtPoint - hit.point).magnitude;

                        //if the object we hit is closer than our min camera distance for the framing, 
                        //we need to check if it is least bad option so far
                        if (distToHit < distanceToCamera.ideal && opacity > Opacity.Medium)
                        {
                            float badness = Mathf.Abs(verticalSearchAngleCurrent - verticalSearchAngleInitial) +
                                            Mathf.Abs(horizontalSearchAngleCurrent) +
                                            Mathf.Abs((distanceToCamera.ideal - distToHit) / distanceToCamera.ideal);
                            if (badness < result.Badness) //update result with new position
                            {
                                result.Badness = badness;
                                //adjust position slightly off intercepted collider toward the subject
                                var cameraToSubjectOffset = (targetLookAtPoint - hit.point).normalized * 0.1f;
                                result.Position = hit.point + cameraToSubjectOffset;
                            }
                        }
                        else
                        {
                            float badness = Mathf.Abs(verticalSearchAngleCurrent - verticalSearchAngleInitial) + getOpacityBadness(opacity);
                            if (badness < result.Badness) //update result with new position
                            {
                                result.Badness = badness;
                                result.Position = targetLookAtPoint + subjectToCamera * distanceToCamera.ideal;
                            }

                        }
                    }
                    else //if we get no hit, there is nothing that can occlude the camera position
                    {
                        //place camera at ideal distance along subjectToCamera vector
                        float badness = verticalSearchAngleCurrent + horizontalSearchAngleCurrent;
                        if (badness < result.Badness) //update result with new position
                        {
                            result.Badness = badness;
                            result.Position = targetLookAtPoint + subjectToCamera * distanceToCamera.ideal;
                        }
                    }
                    if (result.Badness < BADNESS_THRESHOLD)
                    {
                        subjectVisible = true;
                        break;
                    }
                    //unless we find a very close match and break out of this loop, we will update the search angles
                    verticalSearchAngleCurrent += verticalSearchStepSize;
                    verticalSearchIterations++;
                }
                if (!subjectVisible)//search around the circle
                {
                    horizontalSearchSign = -horizontalSearchSign;
                    horizontalSearchAngleCurrent = horizontalSearchSign * horizontalSearchIterations * horizontalSearchStepSize;

                    if (Mathf.Abs(horizontalSearchAngleCurrent) > 1.8 * maxHorizontalSearchPercent) //have we gone more than the allotted amount around the circle?
                    {
                        break;
                    }
                }
            }
            return subjectVisible;
        }

        private float getOpacityBadness(Opacity opacity)
        {
            switch (opacity)
            {
                case Opacity.None:
                    return 0f;
                case Opacity.Low:
                    return 1f;
                case Opacity.Medium:
                    return 2.5f;
                case Opacity.High:
                    return 100f;
            }
            return 1000f;
        }

        class CameraPositionAndLens
        {

            public Vector3 Position { get; set; }
            public ushort LensIndex { get; set; }
            public float Badness { get; set; }
        }

        /// <summary>
        /// calculates orientation of the camera about the subject's y axis
        /// incorporates "direction" and facing of the subject
        /// </summary>
        /// <param name="direction">toward,away,etc for the shot</param>
        /// <param name="defaultTarget">fallback actor in case direction target not found</param>
        /// <returns>radians about Y axis of direction target along which to align camera</returns>
        float getBaseCameraHeading(Direction direction, GameObject defaultTarget)
        {
            float directionBasedYRotation = 0;
            if (direction != null)
            {
                GameObject directionTarget;
                if (getActorByName(direction.Target, out directionTarget) &&
                    directionTarget != null)
                {
                    switch (direction.Heading)
                    {
                        case Heading.Toward:
                            ;//exists for completeness.  
                            break;
                        case Heading.Away:
                            directionBasedYRotation = Mathf.PI;
                            break;
                        case Heading.Left:
                            directionBasedYRotation = -Mathf.PI / 2;
                            break;
                        case Heading.Right:
                            directionBasedYRotation = Mathf.PI / 2;
                            break;
                        default:
                            break;
                    }
                    //add on the rotation from the direction target's orientation about y
                    directionBasedYRotation += directionTarget.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                }
            }
            else //get the root y rotation from the framing target
            {
                directionBasedYRotation += defaultTarget.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            }
            return directionBasedYRotation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="framingTarget">camera's subject</param>
        /// <param name="angleOffsets">angles in radians to supplement direction specification</param>
        /// <returns>normalized 3d vector from target toward camera position</returns>
        Vector3 get3DDirection(GameObject framingTarget, Vector3 angleOffsets, float baseHeading)
        {
            //convert from spherical to rectangular coordinates
            float theta = angleOffsets.y + baseHeading;
            float phi = Mathf.PI / 2 - angleOffsets.x;
            var subjectToCamera = new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi),
                                          Mathf.Cos(phi), //not using traditional mathematical coordinates, swapping z and y calculations
                                          Mathf.Sin(theta) * Mathf.Sin(phi));
            return subjectToCamera.normalized;
        }

        /// <summary>
        /// capturing state for Undo()'ing
        /// </summary>
        private void savePreviousCameraState()
        {
            previousCameraOrientation = camera.transform.rotation;
            previousCameraPosition = camera.transform.position;
            previousLensIndex = (ushort)cameraBody.IndexOfLens;
            previousFStopIndex = (ushort)cameraBody.IndexOfFStop;
            previousFocusDistance = cameraBody.FocusDistance;
        }

        private bool findCamera()
        {

            if (camera == null &&
                !getActorByName(cameraName, out camera))
            {
                Debug.LogError(string.Format("could not find camera[{0}] at time d:s[{1}:{2}].  This is really bad.  What did you do to the camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                return false;
            }

            cameraBody = camera.GetComponentInChildren<CameraBody>();
            if (cameraBody == null)
            {
                Debug.LogError(string.Format("could not find cameraBody component as child of camera[{0}] at time d:s[{1}:{2}].  Why isn't your camera a cinema suites camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                return false;
            }
            return true;
        }

        private bool calculateFocusPosition(string focusTarget, out Vector3 focusPosition)
        {
            focusPosition = new Vector3();
            if (string.IsNullOrEmpty(focusTarget))
                return false;

            //try to parse target as a coordinate                
            if (focusTarget.TryParseVector3(out focusPosition))
            {
                Extensions.Log("focus @" + focusPosition);
                return true;
            }

            //try to find the target as an actor
            GameObject target;
            if (!getActorByName(focusTarget, out target))
            {
                Extensions.Log("actor name [" + focusTarget + "] not found. cannot change focus");
                return false;
            }
            focusPosition = target.transform.position;
            //Extensions.Log("focus target[{0}] @{1} tracking[{2}]", focusTarget, target.transform.position));

            return true;
        }

        private bool calculateAnchor(string anchor, out Vector2 anchorPosition)
        {
            anchorPosition = new Vector2();
            //if there's nothing there, then nothing to ground to
            if (string.IsNullOrEmpty(anchor)) return false;
            Vector2 planarCoords;
            if (anchor.TryParsePlanarCoords(out planarCoords))
            {
                //we can read the anchor string as planar coords
                anchorPosition = planarCoords;
                return true;
            }
            else
            {
                //we can't read anchor string as planar coords.  hopefully this is the name of an actor
                GameObject actorToAnchorOn;

                if (!getActorByName(anchor, out actorToAnchorOn))
                {
                    //sadly there is no such thing.  we should complain and then try to get on with business
                    Debug.LogError(string.Format("anchor actor [{0}] not found at time d:s[{1}:{2}].  calculating anchor freely.",
                        anchor, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                    return false;
                }
                Vector3 actorPosition = actorToAnchorOn.transform.position;
                anchorPosition = new Vector2(actorPosition.x, actorPosition.z);
                return true;
            }
        }

        public override void Execute(float currentTime)
        {
            //nothing to see here.  this is all instant
        }

        public override void Stop()
        {
            //nothing to do and nothing to stop
        }

        public override void Undo()
        {
            camera.transform.position = previousCameraPosition;
            camera.transform.rotation = previousCameraOrientation;
            cameraBody.IndexOfLens = previousLensIndex;
            cameraBody.IndexOfFStop = previousFStopIndex;
            cameraBody.FocusDistance = previousFocusDistance;

        }

        public override void Skip()
        {
            //since this action always happens instantaneously we can assume that the 
            //skip will get run anytime it's selected for addition in the 
            //executing queue in el Presidente
            camera.transform.position = newCameraPosition;
            camera.transform.rotation = newCameraOrientation;
            cameraBody.IndexOfLens = newLensIndex;
            cameraBody.IndexOfFStop = newFStopIndex;
            cameraBody.FocusDistance = newfocusDistance;
        }

    }
}
