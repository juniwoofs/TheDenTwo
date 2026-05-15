player-requirement-trait-reason = Must{$inverted ->
    [true] {" "}not
   *[false] {""}
} have {$constraint} of these traits: {$traits}

player-requirement-species-reason = Must{$inverted ->
    [true] {" "}not
   *[false] {""}
} be one of these species: {$species}

player-requirement-age-reason = Must{$inverted ->
    [true] {" "}not
   *[false] {""}
} be {$constraint} years old.
