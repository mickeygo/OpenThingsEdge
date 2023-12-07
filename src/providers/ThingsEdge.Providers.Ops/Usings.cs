global using System.Collections.Concurrent;
global using System.Diagnostics.CodeAnalysis;
global using System.Net.NetworkInformation;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using MediatR;
global using Nito.AsyncEx;

global using ThingsEdge.Common.Utils;
global using ThingsEdge.Common.DependencyInjection;
global using ThingsEdge.Contracts;
global using ThingsEdge.Contracts.Variables;