using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapPost("/compile", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var pseudocodeRequest = System.Text.Json.JsonSerializer.Deserialize<PseudocodeRequest>(requestBody);

    var result = CompilePseudocode(pseudocodeRequest.pseudocode);

    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});

app.Run();

CompileResult CompilePseudocode(string pseudocode)
{
    if (string.IsNullOrWhiteSpace(pseudocode))
    {
        return new CompileResult { IsSuccess = false, Errors = "El pseudocódigo no puede estar vacío." };
    }

    var errors = new List<string>();
    var variables = new Dictionary<string, double>();
    var output = new List<string>();
    var lines = pseudocode.Split('\n');

    foreach (var line in lines)
    {
        var trimmedLine = line.Trim();

        // Declarar variable
        if (Regex.IsMatch(trimmedLine, @"^declarar variable \w+$"))
        {
            var parts = trimmedLine.Split(' ');
            var variableName = parts[2];
            variables[variableName] = 0;
        }
        // Asignar valor a variable
        else if (Regex.IsMatch(trimmedLine, @"^asignar \d+(\.\d+)? a \w+$"))
        {
            var parts = trimmedLine.Split(' ');
            var value = double.Parse(parts[1]);
            var variableName = parts[3];
            if (variables.ContainsKey(variableName))
            {
                variables[variableName] = value;
            }
            else
            {
                errors.Add($"Variable '{variableName}' no declarada.");
            }
        }
        // Operaciones matemáticas
        else if (Regex.IsMatch(trimmedLine, @"^\w+ = (raiz\(\w+\)|\w+ \w+ \w+ [\+\-\*/])$"))
        {
            var parts = trimmedLine.Split(' ');
            var resultVar = parts[0];
            var operation = parts[2];

            if (operation.StartsWith("raiz"))
            {
                var match = Regex.Match(operation, @"raiz\((\w+)\)");
                if (match.Success)
                {
                    var varName = match.Groups[1].Value;
                    if (variables.ContainsKey(varName))
                    {
                        variables[resultVar] = Math.Sqrt(variables[varName]);
                    }
                    else
                    {
                        errors.Add($"Variable '{varName}' no declarada.");
                    }
                }
            }
            else
            {
                var var1 = parts[2];
                var op = parts[3];
                var var2 = parts[4];

                if (variables.ContainsKey(var1) && variables.ContainsKey(var2))
                {
                    double value1 = variables[var1];
                    double value2 = variables[var2];
                    double result = op switch
                    {
                        "+" => value1 + value2,
                        "-" => value1 - value2,
                        "*" => value1 * value2,
                        "/" => value1 / value2,
                        _ => throw new InvalidOperationException("Operador desconocido")
                    };
                    variables[resultVar] = result;
                }
                else
                {
                    errors.Add($"Variables '{var1}' o '{var2}' no declaradas.");
                }
            }
        }
        // Máximo común múltiplo
        else if (Regex.IsMatch(trimmedLine, @"^\w+ = mcm\(\w+, \w+\)$"))
        {
            var match = Regex.Match(trimmedLine, @"^(\w+) = mcm\((\w+), (\w+)\)$");
            if (match.Success)
            {
                var resultVar = match.Groups[1].Value;
                var var1 = match.Groups[2].Value;
                var var2 = match.Groups[3].Value;

                if (variables.ContainsKey(var1) && variables.ContainsKey(var2))
                {
                    int value1 = (int)variables[var1];
                    int value2 = (int)variables[var2];
                    int mcm = Lcm(value1, value2);
                    variables[resultVar] = mcm;
                }
                else
                {
                    errors.Add($"Variables '{var1}' o '{var2}' no declaradas.");
                }
            }
        }
        // Imprimir variable o texto
        else if (Regex.IsMatch(trimmedLine, @"^imprimir (\w+|\"".+\"")$"))
        {
            var match = Regex.Match(trimmedLine, @"^imprimir (\"".+\""|\w+)$");
            if (match.Success)
            {
                var toPrint = match.Groups[1].Value;
                if (toPrint.StartsWith("\"") && toPrint.EndsWith("\""))
                {
                    output.Add(toPrint.Trim('"'));
                }
                else if (variables.ContainsKey(toPrint))
                {
                    output.Add(variables[toPrint].ToString());
                }
                else
                {
                    errors.Add($"Variable '{toPrint}' no declarada.");
                }
            }
        }
        else if (!Regex.IsMatch(trimmedLine, @"^inicio programa|fin programa$"))
        {
            errors.Add($"Sintaxis no válida en la línea: '{trimmedLine}'");
        }
    }

    return errors.Count > 0
        ? new CompileResult { IsSuccess = false, Errors = string.Join("\n", errors) }
        : new CompileResult { IsSuccess = true, Output = string.Join("\n", output) };
}

static int Gcd(int a, int b)
{
    return b == 0 ? a : Gcd(b, a % b);
}

static int Lcm(int a, int b)
{
    return Math.Abs(a * b) / Gcd(a, b);
}

record PseudocodeRequest
{
    public string pseudocode { get; set; }
}

record CompileResult
{
    public bool IsSuccess { get; set; }
    public string Output { get; set; }
    public string Errors { get; set; }
}