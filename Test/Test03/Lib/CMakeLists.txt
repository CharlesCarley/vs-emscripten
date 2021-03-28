

add_library(MockAPI 
   MockLibAPI.cpp  
   MockLibAPI.h  
   MockLibAPI_impl.cpp	   
)
set_target_properties(MockAPI PROPERTIES VS_GLOBAL_VerboseOutput NoVerboseOutput)
set_target_properties(MockAPI PROPERTIES VS_GLOBAL_EchoCommandLines true)

#set_target_properties(MockAPI PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
#set_target_properties(MockAPI PROPERTIES VS_GLOBAL_DebugTask true)
