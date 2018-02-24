cat neworder.json | grpcurl -k call localhost:8080 PartialFoods.Services.OrderCommand.SubmitOrder

