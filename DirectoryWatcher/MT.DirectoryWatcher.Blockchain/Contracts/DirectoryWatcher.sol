pragma solidity ^0.7.0;

contract DirectoryWatcherContract
{
    address owner;
    
    struct TamperedFile{
        string _fileName;
        string _parentDirectory;
        string _directoryPath;
        string _oldHash;
        string _newHash;
        string _fileChange;
        address _modifierAddress;
    }

    struct TamperedDirectory{
        string _directoryName;
        string _directoryPath;
        string _directoryChange;
        address _modifierAddress;
    }
    
    TamperedFile[] public TamperedFileList;
    TamperedDirectory[] public TamperedDirectoryList;
    uint256 public TamperedFilesCount;
    uint256 public TamperedDirectoriesCount;
    
    modifier onlyOwner(){
        require(msg.sender == owner);
        _;
    }
    
     constructor() public{
        owner = msg.sender;
    }
    
    function InsertTamperedFile(
        string memory fileName,
        string memory parentDirectory,
        string memory directoryPath,
        string memory oldHash,
        string memory newHash,
        string memory fileChange) public
    {
        address modifierAddress = msg.sender;
        TamperedFileList.push(TamperedFile(fileName,parentDirectory,directoryPath,oldHash,newHash,fileChange,modifierAddress));
        TamperedFilesCount++;
    }

     function InsertTamperedDirectory(
        string memory directoryName,
        string memory directoryPath,
        string memory directoryChange) public
    {
        address modifierAddress = msg.sender;
        TamperedDirectoryList.push(TamperedDirectory(directoryName,directoryPath,directoryChange,modifierAddress));
        TamperedDirectoriesCount++;
    }
        
    function FetchTamperedFiles(uint256 index) public view 
        returns(
        string memory fileName,
        string memory parentDirectory,
        string memory directoryPath,
        string memory oldHash,
        string memory newHash,
        string memory fileChange,
        address modifierAddress)
    {
        fileName = TamperedFileList[index]._fileName;
        parentDirectory = TamperedFileList[index]._parentDirectory;
        directoryPath = TamperedFileList[index]._directoryPath;
        oldHash = TamperedFileList[index]._oldHash;
        newHash = TamperedFileList[index]._newHash;
        fileChange = TamperedFileList[index]._fileChange;
        modifierAddress = TamperedFileList[index]._modifierAddress;
        
        return (fileName,parentDirectory,directoryPath,oldHash,newHash,fileChange,modifierAddress);
    }

    function FetchTamperedDirectories(uint256 index) public view 
        returns(
        string memory directoryName,
        string memory directoryPath,
        string memory directoryChange,
        address modifierAddress)
    {
        directoryName = TamperedDirectoryList[index]._directoryName;
        directoryPath = TamperedDirectoryList[index]._directoryPath;
        directoryChange = TamperedDirectoryList[index]._directoryChange;
        modifierAddress = TamperedDirectoryList[index]._modifierAddress;
        
        return (directoryName,directoryPath,directoryChange,modifierAddress);
    }
    
    function FetchListCounts() public view 
        returns (
            uint256 tamperedFilesCount,
            uint256 tamperedDirectoriesCount)
    {
        tamperedFilesCount = TamperedFilesCount;
        tamperedDirectoriesCount = TamperedDirectoriesCount;
        return (tamperedFilesCount, tamperedDirectoriesCount);
    }
}

