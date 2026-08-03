#!/usr/bin/env python3
"""Calculate a Jellyfin release MD5 and update manifest.json."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import zipfile
from datetime import datetime, timezone
from pathlib import Path


EXPECTED_ARTIFACT = "Jellyfin.Plugin.ProviderStuff.dll"
FOUR_PART_VERSION = re.compile(r"^\d+\.\d+\.\d+\.\d+$")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--repository", required=True, help="GitHub owner/repository")
    parser.add_argument("--version", required=True)
    parser.add_argument("--target-abi", required=True)
    return parser.parse_args()


def fail(message: str) -> None:
    raise SystemExit(message)


def md5(path: Path) -> str:
    digest = hashlib.md5(usedforsecurity=False)
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def main() -> int:
    args = parse_args()

    if not FOUR_PART_VERSION.fullmatch(args.version):
        fail("Version must have four numeric parts, for example 1.2.1.0.")
    if not FOUR_PART_VERSION.fullmatch(args.target_abi):
        fail("targetAbi must have four numeric parts, for example 10.11.0.0.")
    if "/" not in args.repository or args.repository.startswith("/"):
        fail("Repository must use the owner/repository format.")
    if not args.archive.is_file():
        fail(f"Archive not found: {args.archive}")

    with zipfile.ZipFile(args.archive) as archive:
        entries = [entry.filename for entry in archive.infolist() if not entry.is_dir()]
    if entries != [EXPECTED_ARTIFACT]:
        fail(f"Archive must contain only {EXPECTED_ARTIFACT}; found: {entries}")

    checksum = md5(args.archive)
    timestamp = datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")
    asset_name = args.archive.name
    source_url = (
        f"https://github.com/{args.repository}/releases/download/"
        f"{args.version}/{asset_name}"
    )

    manifest = json.loads(args.manifest.read_text(encoding="utf-8"))
    if not isinstance(manifest, list) or len(manifest) != 1:
        fail("manifest.json must contain exactly one plugin entry.")

    plugin = manifest[0]
    plugin["version"] = args.version
    plugin["targetAbi"] = args.target_abi
    plugin["framework"] = "net9.0"
    plugin["owner"] = args.repository.split("/", maxsplit=1)[0]

    release = {
        "version": args.version,
        "changelog": f"- Compatibilidade com Jellyfin 10.11.x\n",
        "targetAbi": args.target_abi,
        "sourceUrl": source_url,
        "checksum": checksum,
        "timestamp": timestamp,
    }
    previous_versions = [
        item for item in plugin.get("versions", [])
        if item.get("version") != args.version
    ]
    plugin["versions"] = [release, *previous_versions]

    args.manifest.write_text(
        json.dumps(manifest, ensure_ascii=False, indent=4) + "\n",
        encoding="utf-8",
    )

    output_path = os.environ.get("GITHUB_OUTPUT")
    if output_path:
        with Path(output_path).open("a", encoding="utf-8") as output:
            output.write(f"checksum={checksum}\n")
            output.write(f"source_url={source_url}\n")

    print(f"MD5: {checksum}")
    print(f"Source URL: {source_url}")
    print(f"Updated: {args.manifest}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
