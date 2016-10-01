REM $(SolutionDir)..\unity\Assets\Plugins\x86_64\
REM copy "$(SolutionDir)..\unity\Assets\Plugins\x86_64\*" "$(TargetDir)"
setlocal EnableDelayedExpansion

REM input
set solution_dir=%1
set project_dir=%2
set output_dir=%3
set dll_dir="%solution_dir%thirdparty\dll\imageprocessing\*"

REM copy to unity
set unity_dir="%solution_dir%..\unity\Assets\Plugins\generated"
copy %output_dir%*.dll %unity_dir%
copy %dll_dir% %unity_dir%

REM copy to GUI
set imp_test_dir="%solution_dir%build\bin\gui\"
copy "%output_dir%*.dll" "%imp_test_dir%"
copy %dll_dir% "%imp_test_dir%"


