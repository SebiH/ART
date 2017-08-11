#include <string>
#include <iostream>

extern int calibCamera(int argc, char *argv[]);
extern int calibStereo(int argc, char *argv[]);

int main(int argc, char *argv[])
{
    calibCamera(argc, argv);

	//if (argc >= 2 && std::string(argv[1]) == std::string("calibCamera"))
	//{
	//	calibCamera(argc - 1, argv + 1);
	//}
	//else if (argc >= 2 && std::string(argv[1]) == std::string("calibStereo"))
	//{
	//	calibStereo(argc - 1, argv + 1);
	//}
	//else
	//{
	//	std::cout << "Unknown function" << std::endl;
	//}
}
