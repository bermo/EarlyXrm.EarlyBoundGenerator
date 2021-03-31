CrmSvcUtil.exe ^
 /connectionstring:%1 ^
 /solutionname:%2 ^
 /namespace:%3 ^
 /extra:%4 ^
 /skip:%5 ^
 /usedisplaynames:%6 ^
 /debugMode:%7 ^
 /instrument:%8 ^
 /out:..\..\EarlyEntities.cs ^
  /codewriterfilter:"EarlyXrm.EarlyBoundGenerator.EntitiesCodeFilteringService, EarlyXrm.EarlyBoundGenerator" ^
 /codecustomization:"EarlyXrm.EarlyBoundGenerator.EntitiesCodeCustomistationService, EarlyXrm.EarlyBoundGenerator" ^
     /namingservice:"EarlyXrm.EarlyBoundGenerator.EntitiesCodeNamingService, EarlyXrm.EarlyBoundGenerator"

CrmSvcUtil.exe ^
 /connectionstring:%1 ^
 /solutionname:%2 ^
 /namespace:%3 ^
 /extra:%4 ^
 /skip:%5 ^
 /usedisplaynames:%6 ^
 /debugMode:%7 ^
 /instrument:%8 ^
 /out:..\..\EarlyOptionSets.cs ^
  /codewriterfilter:"EarlyXrm.EarlyBoundGenerator.OptionSetsFilteringService, EarlyXrm.EarlyBoundGenerator" ^
 /codecustomization:"EarlyXrm.EarlyBoundGenerator.OptionSetsCodeCustomisationService, EarlyXrm.EarlyBoundGenerator" ^
     /namingservice:"EarlyXrm.EarlyBoundGenerator.OptionSetsNamingService, EarlyXrm.EarlyBoundGenerator"