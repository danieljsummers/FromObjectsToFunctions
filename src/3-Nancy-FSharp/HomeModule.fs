namespace Tres

open Nancy

type HomeModule() as this =
  inherit NancyModule()

  do
    this.Get("/", fun _ -> "Hello World from Nancy F#")
