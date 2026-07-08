using UnityEngine;

namespace Torpedo
{
    public static class TorpedoPhysics
    {



        public static bool IsOverWater(Missile missile)
        {
            return missile.GlobalPosition().y <= 2f;
        }

        public static bool IsUnderWater(Missile missile)
        {
            return missile.GlobalPosition().y <= 0f;
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
            float dampK = 10f;
            float forceY = yError * springK - missile.rb.velocity.y * dampK;





            if (liveGlobalY <= 0f && yError < 0f && missile.rb.velocity.y <= 0f)
            {
                forceY = Mathf.Max(forceY, -9.8f);
            }

            missile.rb.AddForce(new Vector3(0f, forceY, 0f), ForceMode.Acceleration);


            if (liveGlobalY >= targetGlobalY && missile.rb.velocity.y > 0f)
            {
                missile.rb.velocity = new Vector3(missile.rb.velocity.x, 0f, missile.rb.velocity.z);
            }


            Vector3 tiltAxis = Vector3.Cross(missile.transform.up, Vector3.up);
            if (tiltAxis.sqrMagnitude > 0.0001f)
            {
                missile.rb.AddTorque(tiltAxis * 50f, ForceMode.Acceleration);
            }

            missile.rb.AddTorque(-missile.rb.angularVelocity * 1f, ForceMode.Acceleration);



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

