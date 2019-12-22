# ClipFileWatcher
Watches `.clip` files and commits changes

This is a simple program automatically commit changes made to a `.clip` file.

The idea is to integrate with `gitclip` and keep track of all changes made to a `.clip` file.

## Installation

1. Install `git` https://git-scm.com/
2. Install `LFS` for `git` https://git-lfs.github.com/
3. Install `gitclip` https://github.com/megalon/gitclip
4. Download the latest release from this repo. If unavailable, build from source!

## Usage
1. Select a `.clip` file you want ClipFileWatcher to watch.
2. Simply leave ClipFileWatcher open while you are working on your `.clip` file, and it will automatically commit changes to the git repo whenever you save the file!

ClipFileWatcher will remember the file you selected next time you open it.

That's it!

## Really, that's it?
Yes. GitClip uses a git repo to track changes for `.clip` files. All ClipFileWatcher does is automatically commit changes to that repo whenever it detects that the `.clip` file is modified.

If no `.git` folder is found in the directory the `.clip` file is in, then ClipFileWatcher will attempt to initialize a git repo and setup `gitclip` to track the selected `.clip` file for you.
