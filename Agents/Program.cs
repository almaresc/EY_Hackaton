// Copyright (c) Microsoft. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Agents;
using Agents.Entities;
using Agents.Plugins;
using Azure.AI.Projects;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace AgentsSample;

public static class Program
{
    public static async Task Main()
    {
        // Load configuration from environment variables or user secrets.
        Settings settings = new();
               
        IKernelBuilder builder = Kernel.CreateBuilder();        

        builder.AddAzureOpenAIChatCompletion(
            settings.AzureOpenAI.ChatModelDeployment,
            settings.AzureOpenAI.Endpoint,
            settings.AzureOpenAI.ApiKey);
          

        Kernel kernel = builder.Build();
        Kernel kernel1 = builder.Build();
        Kernel kernel2 = builder.Build();


        AIProjectClient client = AzureAIAgent.CreateAzureAIClient("francecentral.api.azureml.ms;fdf918e9-2704-44c5-895d-ec462e3494f9;rg-almarescai;almaresc-0245", new AzureCliCredential());
        AgentsClient agentsClient = client.GetAgentsClient();

        Azure.AI.Projects.Agent definition = await agentsClient.GetAgentAsync("asst_zEqXEBwQujNtybjYCRTsBA30");
        AzureAIAgent agent = new(definition, agentsClient);
        //agent.Description = "an agent that can response to question on internal process.";

        //Kernel kernel_p1 = builder.Build();
        //Kernel kernel_p2 = builder.Build();

        //var arbitroPls = KernelPluginFactory.CreateFromObject(new ArbitroPlugins());
        //var p1Pls = KernelPluginFactory.CreateFromObject(new PlayerPlugins());
        //var p2Pls = KernelPluginFactory.CreateFromObject(new Player2Plugins());

        const string DocsAgentName = "DocsAgent";
        const string CheckCongruencyAgentName = "CheckUncongruencyAgent";
        const string CriticalAgentName = "CriticalAgent";
        const string RouterAgentName = "SupervisorAgent";

        ChatCompletionAgent RouterAgent =
            new()
            {
                Name = RouterAgentName,
                Instructions = Instractions.RouterAgent,
                Kernel = kernel,
                Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })

            };

       
        //ChatCompletionAgent DocsAgent =
        //    new()
        //    {
        //        Name = DocsAgentName,
        //        Instructions = Instractions.DocsAgent,
        //        Kernel = kernel,
        //        Arguments = new KernelArguments(
        //        new AzureOpenAIPromptExecutionSettings()
        //        {
        //            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        //        })

        //    };
       

        ChatCompletionAgent CheckCongruencyAgent =
            new()
            {
                Name = CheckCongruencyAgentName,
                Instructions = Instractions.CheckCongruencyAgent,
                Kernel = kernel1,
                Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

        kernel1.Plugins.Add(KernelPluginFactory.CreateFromObject(new DocsPlugins()));
       
        ChatCompletionAgent CriticalAgent =
            new()
            {
                Name = CriticalAgentName,
                Instructions = Instractions.CriticalAgent,
                Kernel = kernel2,
                Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

        kernel2.Plugins.Add(KernelPluginFactory.CreateFromObject(new CriticalPlugins()));

        var agentPlugin = KernelPluginFactory.CreateFromFunctions("AgentPlugin",
          [
              AgentKernelFunctionFactory.CreateFromAgent(CriticalAgent),
                AgentKernelFunctionFactory.CreateFromAgent(CheckCongruencyAgent),
                AgentKernelFunctionFactory.CreateFromAgent(agent),
          ]);
        kernel.Plugins.Add(agentPlugin);
        kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter());

        Console.Write("User > ");
        var query = Console.ReadLine();
        Microsoft.SemanticKernel.Agents.AgentThread? agentThread = null;

        while (query != "EXIT")
        {         

            var responseItems = RouterAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, query), agentThread);
            await foreach (var responseItem in responseItems)
            {
                agentThread = responseItem.Thread;
                Console.WriteLine($"{responseItem.Message.AuthorName}> {responseItem.Message.Content}");
            }

            Console.Write("User > ");
            query = Console.ReadLine(); ;
        }

        #region Old
        //AgentGroupChat chat =
        //       new(RouterAgent, CriticalAgent, CheckCongruencyAgent, DocsAgent)
        //       {
        //           ExecutionSettings =
        //            new()
        //            {

        //                //SelectionStrategy = CustomSelectionStrategy.getStrategy(kernel_agent, RouterAgent),
        //                //SelectionStrategy = CustomSelectionStrategy.getSeqStrategy( arbitro),
        //                // Here a TerminationStrategy subclass is used that will terminate when
        //                // an assistant message contains the term "approve".
        //                TerminationStrategy =
        //                    new EndedTerminationStrategy()
        //                    {
        //                        // Only the art-director may approve.
        //                        Agents = [RouterAgent],
        //                        // Limit total number of turns
        //                        MaximumIterations = 2,
        //                    }
        //            }                   
        //       };




        //Console.Write("User > ");
        //var query = Console.ReadLine();

        ////ChatMessageContent message = new(AuthorRole.User, query);
        ////chat.AddChatMessage(message);

        ////while (query != "EXIT")
        ////{

        ////    await foreach (ChatMessageContent response in chat.InvokeAsync())
        ////    {
        ////        Console.WriteLine($"{response.AuthorName}:");
        ////        Console.WriteLine($"{response.Content}");
        ////    }

        ////    message = new(AuthorRole.Assistant, chat.ge);
        ////    chat.AddChatMessage(message);
        ////}
        #endregion

    }



}

public sealed class AutoFunctionInvocationFilter() : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"Invoke: {context.Function.Name}");

        await next(context);
    }
}