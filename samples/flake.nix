{
  description = "Minimal test case for NixFodExporter";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  };

  outputs = { nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages.${system};
    in {
      packages.${system} = {
        flatFod = pkgs.fetchurl {
          url = "https://raw.githubusercontent.com/NixOS/nix/master/README.md";
          sha256 = "1d8q663p5hpr5ly12n7zrqsqw06j4nndxsq6lh568sh6fchhy3z6";
        };
        recursiveFod = pkgs.fetchzip {
          url = "https://github.com/NixOS/patchelf/archive/refs/tags/0.18.0.tar.gz";
          sha256 = "1g6scxrq9zvhrm9k00kggl1j34m0a5a40yixc2r8g7c2vhf612v0";
        };
      };
    };
}