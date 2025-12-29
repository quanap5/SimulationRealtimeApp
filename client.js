// Simple Node.js console client for testing SignalR connection
// Run with: node client.js

const signalR = require("@microsoft/signalr");

const hubUrl = "https://localhost:7001/simulationHub";

console.log("🚀 Starting SignalR Console Client...");
console.log(`Connecting to: ${hubUrl}\n`);

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Handle receiving simulation data
connection.on("ReceiveSimulationData", (data) => {
    console.log(`\n📊 [${new Date(data.timestamp).toLocaleTimeString()}] Simulation Data Received:`);
    console.log(`   Iteration: ${data.iterationNumber}`);
    console.log(`   Temperature: ${data.temperature}°C`);
    console.log(`   Pressure: ${data.pressure} Bar`);
    console.log(`   Velocity: ${data.velocity} m/s`);
    console.log(`   Energy: ${data.energy} J`);
    console.log(`   Status: ${data.status}`);
});

// Handle simulation started event
connection.on("SimulationStarted", (message) => {
    console.log(`\n✅ ${message}`);
});

// Handle simulation stopped event
connection.on("SimulationStopped", (message) => {
    console.log(`\n⏸️  ${message}`);
});

// Handle reconnection
connection.onreconnecting((error) => {
    console.log(`\n🔄 Connection lost. Reconnecting...`);
    if (error) {
        console.log(`   Error: ${error.message}`);
    }
});

connection.onreconnected((connectionId) => {
    console.log(`\n✅ Reconnected successfully!`);
    console.log(`   Connection ID: ${connectionId}`);
});

connection.onclose((error) => {
    console.log(`\n❌ Connection closed.`);
    if (error) {
        console.log(`   Error: ${error.message}`);
    }
    process.exit(0);
});

// Start connection
async function start() {
    try {
        await connection.start();
        console.log("✅ Connected to SignalR hub successfully!");
        console.log(`Connection ID: ${connection.connectionId}\n`);
        console.log("Listening for simulation data...");
        console.log("Press Ctrl+C to exit\n");
        
        // Optionally start the simulation
        // await connection.invoke("StartSimulation");
        
    } catch (err) {
        console.error("❌ Error connecting to hub:", err.message);
        console.log("\nMake sure the API is running at https://localhost:7001");
        setTimeout(start, 5000);
    }
}

// Handle graceful shutdown
process.on('SIGINT', async () => {
    console.log('\n\n👋 Shutting down...');
    await connection.stop();
    process.exit(0);
});

// Start the client
start();
