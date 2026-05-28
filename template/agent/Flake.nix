{
  description = "Web4app Autonomous Agent Runtime";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

    flake-utils.url = "github:numtide/flake-utils";

    rust-overlay.url = "github:oxalica/rust-overlay";

    devenv.url = "github:cachix/devenv";
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
    rust-overlay,
    devenv,
    ...
  }:
    flake-utils.lib.eachDefaultSystem (system:
      let

        pkgs = import nixpkgs {
          inherit system;

          overlays = [
            (import rust-overlay)
          ];

          config.allowUnfree = true;
        };

        python = pkgs.python311.withPackages (ps:
          with ps; [
            fastapi
            uvicorn
            pydantic
            aiohttp
            websockets
            requests
            rich
            typer
            numpy
            pandas
            pillow
            transformers
            torch
            sentence-transformers
            cryptography
            pyjwt
            web3
          ]);

      in {

        devShells.default = pkgs.mkShell {

          buildInputs = with pkgs; [

            # Core
            git
            curl
            wget
            jq
            tree
            unzip

            # Node
            nodejs_22
            bun
            yarn
            pnpm

            # Python
            python

            # Rust/WASM
            rust-bin.stable.latest.default
            cargo
            rustc
            wasm-pack
            binaryen

            # Go
            go
            gopls

            # Blockchain
            foundry
            solc
            nodePackages.hardhat

            # Databases
            sqlite
            postgresql

            # Containers
            docker
            docker-compose

            # Build
            gcc
            cmake
            pkg-config

            # Security
            openssl

            # Local AI
            ollama
          ];

          shellHook = ''
            echo ""
            echo "⚡ Web4app Runtime Initialized"
            echo "🤖 Agent Runtime Ready"
            echo ""

            export WEB4_ENV=development
            export AGENT_RUNTIME=enabled
            export PYTHONUNBUFFERED=1

            mkdir -p logs
            mkdir -p runtime
            mkdir -p memory
            mkdir -p agents
          '';
        };

        apps.default = {
          type = "app";

          program = "${pkgs.writeShellScript "web4-agent" ''
            if [ -f main.py ]; then
              python main.py

            elif [ -f app.py ]; then
              python app.py

            elif [ -f index.js ]; then
              bun run index.js

            elif [ -f Cargo.toml ]; then
              cargo run

            else
              echo "No runtime entrypoint found."
            fi
          ''}";
        };

      });
}
