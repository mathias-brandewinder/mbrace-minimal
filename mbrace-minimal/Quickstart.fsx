(*
This script launches a local cluster, and
illustrates some basic actions you can 
perform with mbrace.

Run this script block by block, following
the instructions in comments.

For a more comprehensive set of tutorials,
check out the MBrace.StartKit project here:
https://github.com/mbraceproject/MBrace.StarterKit
*)

// 0. Start the cluster.

#load "LocalCluster.fsx"

open MBrace
open MBrace.Core
open MBrace.Flow
open Config

let cluster = Config.GetCluster()


// 1. Get information about the cluster

// Information about cluster workers
cluster.ShowWorkers()
// Information about processes, either
// running or completed.
cluster.ShowProcesses()


// 2. Send work to the cluster for execution,
// using the cloud { ... } computation expression.

let workItem1 = 
    cloud { 
        printfn "DOING SOME WORK"
        return "HELLO" 
        } 

// send for immediate execution, blocking until
// the work completed. On one of the "machines",
// you should see "DOING SOME WORK" being printed.
workItem1 |> cluster.Run

// send for execution, and request the result
// once the work is completed

let process1 = workItem1 |> cluster.CreateProcess

// check for the status of the process:
let status1 = process1.Status

// once status is Completed, the results can be accessed.
// In the meanwhile, your local environment is not blocked.
let result1 = process1.Result


// 3. Sending work to the cluster for parallel execution

// creating 20 work items
let workItems = 
    [ for i in 1 .. 20 ->
        cloud {
            printfn "PROCESSING ITEM %i" i
            return i
            }
    ]

// send them for parallel execution. MBrace will
// distribute the work across the cluster: you should
// see work being processed on multiple machines.
workItems
|> Cloud.Parallel
|> cluster.Run

// review the processes that took place:
cluster.ShowProcesses()


// 4. Handling "distributed sequences" with CloudFlow

// create an array of 1,000 random numbers
let rng = System.Random()
let data = Array.init 1000 (fun _ -> rng.Next(0,10))

// CloudFlow behaves like a "traditional" Sequence, 
// but distributes the work across workers:
let dataAnalysis =
    data
    |> CloudFlow.OfArray
    |> CloudFlow.map (fun x -> x + 1)
    |> CloudFlow.filter (fun x -> x % 2 = 0)
    |> CloudFlow.countBy (fun x -> x)
    |> CloudFlow.toArray

dataAnalysis |> cluster.Run


// 5. Create an in-memory, distributed "array"
// from the data.

// persist the array in memory, partitioned across workers:
let inMemory =
    data
    |> CloudFlow.OfArray
    |> CloudFlow.map (fun x -> x + 1)
    |> CloudFlow.persist (StorageLevel.Memory)
    |> cluster.Run

// you can now work against that array, "as if" it was one,
// but effectively taking place across the cluster:
inMemory
|> CloudFlow.map (fun x ->
    // print out to verify on which worker 
    // the work is taking place
    printfn "SOME WORK HAPPENING HERE"
    x)
|> CloudFlow.filter (fun x -> x % 3 = 0)
|> CloudFlow.distinct
|> CloudFlow.toArray
|> cluster.Run

// This is particularly useful if the data is 
// stored somewhere: you can load it once in memory, 
// and keep it there for quick access.


// 6. Kill the cluster...
Config.KillCluster ()


(*
That's it! Now if you want to go further...

The starter kit contains many example scripts,
covering various tasks, as well as a script to 
help you quickly deploy a "real" cluster on Azure:

https://github.com/mbraceproject/MBrace.StarterKit

This page contains more information on how to
deploy an actual cluster on Azure:
http://mbrace.io/azure-tutorial.html
*) 