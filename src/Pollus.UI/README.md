# Pollus UI

- The UI library was made with the help of AI. The systems and architecture was planned and organized by myself and then made use of Claude in order to implement that plan. Most of the code have been manually reviewed and tweaked but because of the nature of using AI for larger sections like this there are still some less optimal and verbose implementations. With the help of AI the groundwork for future work was pretty quickly implemented, but there are still a bit of work to be done here before its fully ready to use. Code has been reviewed and modified after the fact where it was necessary, and there is still some work left to be done in the layout calculation.
- This choice was primarily made because most features in Pollus is implemented via plugins. This means we can easily remove or avoid using this module if the use of AI is unwanted. The rest of the Pollus engine is made without heavy AI use and this UI module served as an experiment to see what it is capable of.

## Features
- Flex based layout
- CSS inspired styling
- Just the layout
    - SDF based renderer can be found under Pollus.Engine

More information can be found under [DOCS.md](../../DOCS.md).