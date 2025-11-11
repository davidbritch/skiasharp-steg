# Steganography using SkiaSharp

This repo contains a prototype that shows how to perform steganography by hiding information in JPEG images. The sample app is a .NET MAUI app that targets Mac Catalyst, and uses SkiaSharp to load and display images. With a little work the app could also target iOS and Android. Targeting Windows requires more work due to Windows using a different pixel format.

The app performs the following operations:

- Loads images from the photo library.
- Re-encodes the loaded image to JPEG, and hides text in the image.
- Saves stegnagraphic images to the file system.
- Decodes a loaded image, and extracts any text that's been hidden in the image.
 
The app uses the MVVM pattern, with MVVM support coming from CommunityToolkit.Mvvm.

For more information, see the following blog posts:

- [Implementing JPEG encoding in C#](https://davestechlab.co.uk/software/implementing-jpeg-encoding/)
