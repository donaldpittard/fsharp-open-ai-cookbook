module Review

open CsvHelper

type Review() =
    member val Id: int = 0 with get, set
    member val Time: int = 0 with get, set
    member val ProductId: string = "" with get, set
    member val UserId: string = "" with get, set
    member val Score: int = 0 with get, set
    member val Summary: string = "" with get, set
    member val Text: string = "" with get, set

type AugmentedReview() =
    inherit Review()
    member val Combined: string = "" with get, set
    member val NTokens: int = 0 with get, set
    member val Embedding: float32[] = [||] with get, set

    new(baseReview: Review) as this =
        AugmentedReview()
        then
            this.Id <- baseReview.Id
            this.Time <- baseReview.Time
            this.ProductId <- baseReview.ProductId
            this.UserId <- baseReview.UserId
            this.Score <- baseReview.Score
            this.Summary <- baseReview.Summary
            this.Text <- baseReview.Text

    new(copy: AugmentedReview) as this =
        AugmentedReview()
        then
            this.Id <- copy.Id
            this.Time <- copy.Time
            this.ProductId <- copy.ProductId
            this.UserId <- copy.UserId
            this.Score <- copy.Score
            this.Summary <- copy.Summary
            this.Text <- copy.Text
            this.Combined <- copy.Combined
            this.NTokens <- copy.NTokens
            this.Embedding <- copy.Embedding

type StringArrayConverter() =
    inherit TypeConversion.ArrayConverter()

    override _.ConvertFromString(text, row, memberMapData) =
        match text with
        | null -> null
        | _ ->
            let trimmedText = text.Trim('[', ']')
            let elements = trimmedText.Split([| ',' |], StringSplitOptions.RemoveEmptyEntries)
            let parsedArray = Array.map float32 elements

            parsedArray


    override _.ConvertToString(value, row, memberData) =
        let array: float32[] = value :?> float32[]

        match array with
        | null -> null
        | _ ->
            let formattedArray = array |> Array.map string |> String.concat ","

            $"[{formattedArray}]"

type AugmentedReviewMap() as this =
    inherit Configuration.ClassMap<AugmentedReview>()

    do
        this.Map(fun (m: AugmentedReview) -> m.Id) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.Time) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.ProductId) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.UserId) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.Score) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.Summary) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.Text) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.Combined) |> ignore
        this.Map(fun (m: AugmentedReview) -> m.NTokens) |> ignore

        this
            .Map(fun (m: AugmentedReview) -> m.Embedding)
            .TypeConverter<StringArrayConverter>()
        |> ignore
