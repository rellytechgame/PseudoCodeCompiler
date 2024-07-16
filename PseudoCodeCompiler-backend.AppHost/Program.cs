var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PseudoCodeCompiler_backend>("pseudocodecompiler-backend");

builder.Build().Run();
