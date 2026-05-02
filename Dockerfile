FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["# CLINIC APPOINTMENT SYSTEM/ClinicAppointmentSystem.csproj", "# CLINIC APPOINTMENT SYSTEM/"]
RUN dotnet restore "./# CLINIC APPOINTMENT SYSTEM/ClinicAppointmentSystem.csproj"
COPY . .
WORKDIR "/src/# CLINIC APPOINTMENT SYSTEM"
RUN dotnet build "./ClinicAppointmentSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClinicAppointmentSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClinicAppointmentSystem.dll"]
