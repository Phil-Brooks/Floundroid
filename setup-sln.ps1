Write-Host "Creating Floundroid solution..."

# Create solution
dotnet new sln -n Floundroid

# Add projects
$projects = @(
  "src/Floundroid.Core/Floundroid.Core.fsproj",
  "src/Floundroid.UCI/Floundroid.UCI.fsproj",
  "src/Floundroid.Search/Floundroid.Search.fsproj",
  "src/Floundroid.Bitboard/Floundroid.Bitboard.fsproj",
  "src/Floundroid.App/Floundroid.App.fsproj",
  "tests/Floundroid.Tests/Floundroid.Tests.fsproj"
)

foreach ($p in $projects) {
  Write-Host "Adding $p"
  dotnet sln add $p
}

Write-Host "Solution created and all projects added."
