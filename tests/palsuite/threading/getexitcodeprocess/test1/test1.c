/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure GetExitCodeProcess works properly.
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               ZeroMemory
**               GetCurrentDirectoryW
**               CreateProcessW
**               WaitForSingleObject
**               GetLastError
**               strlen
**               strncpy
** 
** 
**  Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
** 
**  The use and distribution terms for this software are contained in the file
**  named license.txt, which can be found in the root of this distribution.
**  By using this software in any fashion, you are agreeing to be bound by the
**  terms of this license.
** 
**  You must not remove this notice, or any other, from this software.
** 
**
**===========================================================================*/
#include <palsuite.h>
#include "myexitcode.h"


static const char* rgchPathDelim = "\\";


int 
mkAbsoluteFilename( LPSTR dirName,  
                    DWORD dwDirLength, 
                    LPCSTR fileName, 
                    DWORD dwFileLength,
                    LPSTR absPathName )
{
    DWORD sizeDN, sizeFN, sizeAPN;

    sizeDN = strlen( dirName );
    sizeFN = strlen( fileName );
    sizeAPN = (sizeDN + 1 + sizeFN + 1);

    /* ensure ((dirName + DELIM + fileName + \0) =< _MAX_PATH ) */
    if( sizeAPN > _MAX_PATH )
    {
        return ( 0 );
    }
    
    strncpy( absPathName, dirName, dwDirLength +1 );
    strncpy( absPathName, rgchPathDelim, 2 );
    strncpy( absPathName, fileName, dwFileLength +1 );

    return (sizeAPN);
  
} 


int __cdecl main( int argc, char **argv ) 

{
    const char* rgchChildFile = "childprocess";
    
    STARTUPINFO si;
    PROCESS_INFORMATION pi;

    DWORD dwError;
    DWORD dwExitCode;
    DWORD dwFileLength;
    DWORD dwDirLength;
    DWORD dwSize;
    
    char  rgchDirName[_MAX_DIR];  
    char  absPathBuf[_MAX_PATH];
    char* rgchAbsPathName;

    /* initialize the PAL */
    if( PAL_Initialize(argc, argv) != 0 )
    {
	    return( FAIL );
    }
    
    /* zero our process and startup info structures */
    ZeroMemory( &si, sizeof(si) );
    si.cb = sizeof( si );
    ZeroMemory( &pi, sizeof(pi) );
    
    /* build the absolute path to the child process */
    rgchAbsPathName = &absPathBuf[0];
    dwFileLength = strlen( rgchChildFile );
    
    dwDirLength = GetCurrentDirectory( _MAX_PATH, rgchDirName );
    if( dwDirLength == 0 ) 
    {
        dwError = GetLastError();
        Fail( "GetCurrentDirectory call failed with error code %d\n", 
              dwError ); 
    }

    dwSize = mkAbsoluteFilename(   rgchDirName,
                                   dwDirLength,
                                   rgchChildFile, 
                                   dwFileLength,
                                   rgchAbsPathName );
    if( dwSize == 0 )
    {
        Fail( "Palsuite Code: mkAbsoluteFilename() call failed.  Could ", 
              "not build absolute path name to file\n.  Exiting.\n" ); 
    }

    /* launch the child process */
    if( !CreateProcess(     NULL,               /* module name to execute */
                            rgchAbsPathName,    /* command line */
                            NULL,               /* process handle not */
                                                /* inheritable */
                            NULL,               /* thread handle not */
                                                /* inheritable */
                            FALSE,              /* handle inheritance */
                            CREATE_NEW_CONSOLE, /* dwCreationFlags */
                            NULL,               /* use parent's environment */
                            NULL,               /* use parent's starting */
                                                /* directory */
                            &si,                /* startup info struct */
                            &pi )               /* process info struct */
        )
    {
        dwError = GetLastError();
        Fail( "CreateProcess call failed with error code %d\n", 
              dwError ); 
    }
    
    /* wait for the child process to complete */
    WaitForSingleObject ( pi.hProcess, INFINITE );

    /* check the exit code from the process */
    if( ! GetExitCodeProcess( pi.hProcess, &dwExitCode ) )
    {
        dwError = GetLastError();
        CloseHandle ( pi.hProcess );
        CloseHandle ( pi.hThread );
        Fail( "GetExitCodeProcess call failed with error code %d\n", 
              dwError ); 
    }
    
    /* close process and thread handle */
    CloseHandle ( pi.hProcess );
    CloseHandle ( pi.hThread );
    
    /* check for the expected exit code */
    if( dwExitCode != TEST_EXIT_CODE )
    {
        Fail( "GetExitCodeProcess returned an incorrect exit code %d, "
              "expected value is %d\n", 
              dwExitCode, TEST_EXIT_CODE ); 
    }

    /* terminate the PAL */
    PAL_Terminate();
    
    /* return success */
    return PASS; 
}
