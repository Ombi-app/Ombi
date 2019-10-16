workflow "New workflow" {
  on = "push"
  resolves = [".NET Core CLI"]
}

action ".NET Core CLI" {
  uses = "baruchiro/github-actions@0.0.1"
  args = "build src/Ombi.sln"
}
