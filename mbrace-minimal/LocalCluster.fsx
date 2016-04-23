(*
This script configures the local cluster.
Change workerCount to modify the cluster,
and run/simulate as many workers as desired.
*)

#I @"../packages/"
#r @"Streams/lib/net45/Streams.dll"
#r @"MBrace.Flow/lib/net45/MBrace.Flow.dll"
#load "MBrace.Thespian/MBrace.Thespian.fsx"

namespace global

module Config = 

    open MBrace.Core
    open MBrace.Runtime
    open MBrace.Thespian

    // change to alter cluster size
    let private workerCount = 4
    
    let mutable private thespian = None
    /// Gets or creates a new Thespian cluster session.
    let GetCluster() = 
        match thespian with 
        | None -> 
            let cluster = 
                ThespianCluster.InitOnCurrentMachine(workerCount, 
                                                     logger = new ConsoleLogger(), 
                                                     logLevel = LogLevel.Info)
            thespian <- Some cluster
        | Some t -> ()
        thespian.Value

    /// Kills the current cluster session
    let KillCluster() =
        match thespian with
        | None -> ()
        | Some t -> t.KillAllWorkers() ; thespian <- None
