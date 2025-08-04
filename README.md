# M3U_to_STRM

## Overview
M3U_to_STRM is a handy utility that parses M3U playlists and converts the contained links into .strm files. These .strm files are organized into folders named after their respective playlists. This tool facilitates the integration of M3U playlists with popular media servers like Jellyfin and Emby.

## Usage
1. **Prepare Your M3U Playlist**: Ensure you have an M3U playlist file with the desired media links.
2. **Set M3U Link**: Open the `url.config` file and save your M3U link. This link will be used by the program to fetch and process the M3U content.
3. **Run M3U_to_STRM**: Execute the M3U_to_STRM program, and it will process the M3U file, creating .strm files for each media item.
4. **Organized Output**: The generated .strm files are stored in folders corresponding to the playlist names, keeping your media neatly organized.

## Configuration
Before running the program, make sure to set up the `url.config` file with your M3U link. Here's how to do it:

1. Open the `url.config` file in a text editor.
2. Replace `YOUR_M3U_LINK_HERE` with the actual URL of your M3U playlist.
3. Save the file.

## Supported Media Servers

### Jellyfin
[Jellyfin](https://jellyfin.org/) is an open-source media server software that allows you to manage, organize, and stream your media content to various devices. It offers a user-friendly interface and extensive features for media enthusiasts.

### Emby
[Emby](https://emby.media/) is a media server solution that offers powerful media management and streaming capabilities. It provides cross-platform support and a rich set of features for organizing and enjoying your media collection.

## Getting Started
To get started, download the M3U_to_STRM program and run it on your media files. You can then configure your favorite media server (Jellyfin or Emby) to include the generated .strm files for seamless media streaming.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
