{
  description = "LMLM / Web4 / Fadaka Unified Runtime";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

    flake-utils.url = "github:numtide/flake-utils";

    rust-overlay.url = "github:oxalica/rust-overlay";

    fenix.url = "github:nix-community/fenix";

    crane.url = "github:ipetkov/crane";

    devenv.url = "github:cachix/devenv";

    systems.url = "github:nix-systems/default";
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
    rust-overlay,
    fenix,
    crane,
    devenv,
    systems,
    ...
  }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        overlays = [
          (import rust-overlay)
        ];

        pkgs = import nixpkgs {
          inherit system overlays;
          config.allowUnfree = true;
        };

        rustToolchain =
          pkgs.rust-bin.stable.latest.default;

        python =
          pkgs.python311.withPackages (ps:
            with ps; [
              fastapi
              uvicorn
              web3
              requests
              numpy
              pandas
              pillow
              transformers
              torch
            ]);

      in
      {
        devShells.default = pkgs.mkShell {

          buildInputs = with pkgs; [

            # Core
            git
            curl
            wget
            jq
            unzip
            zip

            # Node/Bun
            nodejs_22
            bun
            yarn
            pnpm

            # Python
            python

            # Rust
            rustToolchain
            cargo
            rustc

            # Go
            go
            gopls

            # Solidity/Web3
            foundry
            solc
            nodePackages.hardhat

            # Astro/Jekyll
            ruby
            jekyll

            # AI / ML
            ollama

            # Containers
            docker
            docker-compose

            # Build Tools
            cmake
            pkg-config
            gcc

            # Databases
            sqlite

            # WASM
            wasm-pack
            binaryen

            # Networking
            openssl

          ];

          shellHook = ''
            echo ""
            echo "🚀 LMLM Unified Dev Environment"
            echo "⚡ Web4 + Fadaka + AI Runtime Ready"
            echo ""

            export NODE_ENV=development
            export PYTHONUNBUFFERED=1

            export FADAKA_ENV=dev
            export WEB4_RUNTIME=enabled

            alias ll='ls -la'
            alias gs='git status'
            alias serve='python -m http.server 8080'
          '';
        };

        packages.default = pkgs.stdenv.mkDerivation {
          name = "lmlm-runtime";

          src = ./.;

          buildPhase = ''
            echo "Building LMLM Runtime..."
          '';

          installPhase = ''
            mkdir -p $out
            cp -r . $out/
          '';
        };

        apps.default = {
          type = "app";

          program = "${pkgs.writeShellScript "start-lmlm" ''
            echo "Launching LMLM Runtime..."
            bun run dev || npm run dev || python main.py
          ''}";
        };
      });
}
