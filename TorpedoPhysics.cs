using System;
using UnityEngine;

namespace Torpedo
{
    public static class TorpedoPhysics
    {
        public static bool IsOverWater(Missile missile)
        {
            if (Physics.Raycast(missile.transform.position, Vector3.down, out RaycastHit hit, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                float hitGlobalY = (float)hit.point.ToGlobalPosition().y;

                if (Mathf.Abs(hitGlobalY) < 1f)
                    return true;

                if (hitGlobalY > 1f)
                    return false;

                Transform t = hit.collider.transform;
                for (int i = 0; i < 3 && t != null; i++, t = t.parent)
                {
                    if (t.name.IndexOf("terrain2_tile", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                return false;
            }
            return true;
        }

        public static bool InCruisePhase(Missile missile)
        {
            return missile.EngineOn() && missile.GlobalPosition().y <= 1f;
        }

        public static void ApplyTorpedoPhysics(Missile missile, float targetGlobalY)
        {
            if (missile.rb == null) return;
            float liveGlobalY = (float)missile.GlobalPosition().y;
            float yError = targetGlobalY - liveGlobalY;

            float springK = 50f;
            float dampK = 15f;
            float forceY = yError * springK - missile.rb.velocity.y * dampK;

            if (liveGlobalY <= 0f && yError < 0f && missile.rb.velocity.y <= 0f)
            {
                forceY = Mathf.Max(forceY, -9.8f);
            }

            missile.rb.AddForce(new Vector3(0f, forceY, 0f), ForceMode.Acceleration);

            if (liveGlobalY < targetGlobalY - 2f && missile.rb.velocity.y < 0f)
            {
                missile.rb.velocity = new Vector3(missile.rb.velocity.x, 0f, missile.rb.velocity.z);
            }

            missile.rb.AddTorque(-missile.rb.angularVelocity * 5f, ForceMode.Acceleration);

            Vector3 horizVel = new Vector3(missile.rb.velocity.x, 0f, missile.rb.velocity.z);
            float currentSpeed = horizVel.magnitude;
            float minSpeed = 25f;
            
            if (currentSpeed < minSpeed)
            {
                Vector3 forward = missile.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                forward.Normalize();
                
                missile.rb.AddForce(forward * 15f, ForceMode.Acceleration);
            }
        }
    }
}
