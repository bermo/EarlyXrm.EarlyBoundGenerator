Set a=%1 & Set b=%2 & Set c=%3 & Set d=%4 & Set e=%5 & Set f=%6 & Set g=%7 & Set h=%8 & Set i=%9
Shift & Shift & Shift & Shift & Shift & Shift & Shift & Shift & Shift
Set j=%1 & Set k=%2 & Set l=%3

CrmSvcUtil.exe ^
 %a% ^
 /solutionname:%b% ^
 /namespace:%c% ^
 /extra:%d% ^
 /skip:%e% ^
 /usedisplaynames:%f% ^
 /debugMode:%g% ^
 /instrument:%h% ^
 /addsetters:%i% ^
 /out:%j% ^
 /nestnonglobalenums:%k% ^
 /generateconstants:%l% ^
  /codewriterfilter:"EarlyXrm.EarlyBoundGenerator.CodeFilteringService, EarlyXrm.EarlyBoundGenerator" ^
 /codecustomization:"EarlyXrm.EarlyBoundGenerator.CodeCustomistationService, EarlyXrm.EarlyBoundGenerator" ^
     /namingservice:"EarlyXrm.EarlyBoundGenerator.CodeNamingService, EarlyXrm.EarlyBoundGenerator"
