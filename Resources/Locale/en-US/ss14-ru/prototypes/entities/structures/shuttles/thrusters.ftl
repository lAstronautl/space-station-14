ent-BaseThruster = { ent-BaseStructureDynamic }
    .desc = { ent-BaseStructureDynamic.desc }
    .suffix = { "" }
ent-Thruster = thruster

  .desc = { ent-['BaseThruster', 'ConstructibleMachine'].desc }
  .suffix = { "" }
ent-ThrusterUnanchored = { ent-Thruster }
    .desc = { ent-Thruster.desc }
    .suffix = { "" }
ent-DebugThruster = { ent-BaseThruster }
    .suffix = DEBUG
    .desc = { ent-BaseThruster.desc }
ent-Gyroscope = gyroscope

  .desc = { ent-['BaseThruster', 'ConstructibleMachine'].desc }
  .suffix = { "" }
ent-GyroscopeUnanchored = { ent-Gyroscope }
    .desc = { ent-Gyroscope.desc }
    .suffix = { "" }
ent-DebugGyroscope = { ent-BaseThruster }
    .suffix = DEBUG
    .desc = { ent-BaseThruster.desc }
