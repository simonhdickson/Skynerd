namespace Skynerd
open System.Collections.Generic

module Monads =
    module State =
        type State<'s,'a> = State of ('s -> 'a * 's) 

        type StateBuilder<'s>()=
            member m.Bind(State x, f) =
                State(fun s ->
                    let (a,s) = x s
                    let (State x') = f a
                    x' s)
            member m.Return x : State<'s,_> = State(fun s -> x,s)
            member m.ReturnFrom x = x

        let state<'s> = StateBuilder<'s>()

        let getState = State(fun s -> s,s)
        let putState s = State(fun _ -> (),s)
        let executeState (State s) init = s init

//    type OwinBuilder()=
//        member m.Bind(x, f) = f x
//        member m.Return x =
//            let (next,enviroment) = get()
//            next