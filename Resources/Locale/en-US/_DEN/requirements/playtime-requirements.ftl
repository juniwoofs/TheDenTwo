player-requirement-playtime-minimum-time = at least {$playtime}
player-requirement-playtime-maximum-time = less than {$playtime}
player-requirement-playtime-minmax-time = between {$minimum} and {$maximum}
player-requirement-playtime-constraint-reason = Must{$inverted ->
    [true] {" "}not
   *[false] {""}
} have {$constraint} of playtime

player-requirement-department-playtime-reason = {$constraint} in the {$department} department.
player-requirement-job-playtime-reason = {$constraint} as a {$job}.
player-requirement-overall-playtime-reason = {$constraint} overall.
