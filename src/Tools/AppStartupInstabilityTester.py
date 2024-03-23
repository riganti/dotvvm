#!/usr/bin/env python3

import subprocess, requests, os, time, argparse

parser = argparse.ArgumentParser(description="Repeatedly starts the server and every time checks if some pages are working, use to find startup-time race condition bugs")
parser.add_argument("--port", type=int, default=16017, help="Port to run the server on")
parser.add_argument("--working-directory", type=str, default=".", help="Working directory to run the server in")
parser.add_argument("--server-path", type=str, default="bin/Debug/net8.0/DotVVM.Samples.BasicSamples.AspNetCoreLatest", help="Path to the server executable")
parser.add_argument("--environment", type=str, default="Development", help="Asp.Net Core environment (Development, Production)")
args = parser.parse_args()

port = args.port

def server_start() -> subprocess.Popen:
    """Starts the server and returns the process object"""
    server = subprocess.Popen([
        args.server_path, "--environment", args.environment, "--urls", f"http://localhost:{port}"],
        cwd=args.working_directory,
    )
    return server

def req(path):
    try:
        response = requests.get(f"http://localhost:{port}{path}")
        return response.status_code
    except requests.exceptions.ConnectionError:
        return None

iteration = 0
while True:
    iteration += 1
    print(f"Starting iteration {iteration}")
    server = server_start()
    time.sleep(0.1)
    while req("/") is None:
        time.sleep(0.1)

    probes = [
        req("/"),
        req("/FeatureSamples/LambdaExpressions/StaticCommands"),
        req("/FeatureSamples/LambdaExpressions/ClientSideFiltering"),
        req("/FeatureSamples/LambdaExpressions/LambdaExpressions")
    ]
    if set(probes) != {200}:
        print(f"Iteration {iteration} failed: {probes}")
        time.sleep(100000000)

    server.terminate()
    server.wait()
