module OpenAi


open System
open System.Net.Http
open System.Numerics.Tensors
open System.Text

type Models = | TextEmbeddingAda002

type EmbeddingsRequestData = { Input: string; Model: string }

type Embedding =
    { Object: string
      Index: int
      Embedding: float32[] }

type EmbeddingsResponse = { Object: string; Data: Embedding[] }

type OpenAiClient private (httpClient: HttpClient) =
    member _.GetEmbeddings content model =
        let embeddingsUrl = "https://api.openai.com/v1/embeddings"

        let raw = { Input = content; Model = model }

        let jsonSerializationOptions =
            new Json.JsonSerializerOptions(PropertyNamingPolicy = Json.JsonNamingPolicy.CamelCase)

        let jsonData = Json.JsonSerializer.Serialize(raw, jsonSerializationOptions)
        let content = new StringContent(jsonData, Encoding.UTF8, "application/json")

        async {
            let! response = httpClient.PostAsync(embeddingsUrl, content) |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                let embeddingsResult =
                    try
                        let deserializationOptions =
                            new Json.JsonSerializerOptions(PropertyNameCaseInsensitive = true)

                        let embeddingsResponse =
                            Json.JsonSerializer.Deserialize<EmbeddingsResponse>(content, deserializationOptions)

                        Ok embeddingsResponse.Data
                    with ex ->
                        Error ex.Message

                return embeddingsResult
            else
                return Error response.ReasonPhrase
        }

    static member Create(apiToken: string) =
        if String.IsNullOrEmpty apiToken then
            Error "API Token must not be empty"
        else
            let baseUri = new Uri("https://api.openai.com/v1/embeddings")
            let httpClient = new HttpClient()
            httpClient.BaseAddress <- baseUri
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

            Ok(new OpenAiClient(httpClient))

let getClient apiKey =
    OpenAiClient.Create apiKey
    |> (function
    | Ok c -> c
    | Error s -> failwith "Unable to initialize client")

module EmbeddingsUtils =
    let getEmbeddings str model : Result<Embedding[], string> =
        let apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        let client = getClient apiKey
        let result = client.GetEmbeddings str model |> Async.RunSynchronously

        match result with
        | Ok embedding -> Ok(embedding)
        | Error m -> Error m

    let getCosineSimilarity (v1: float32[]) (v2: float32[]) =
        let x = ReadOnlySpan<float32>(v1)
        let y = ReadOnlySpan<float32>(v2)

        TensorPrimitives.CosineSimilarity(x, y)
