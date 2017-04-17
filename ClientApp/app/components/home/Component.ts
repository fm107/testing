export interface IFileSystem {
    fullName: string;
    name: string;
    size: number;
    lastChanged: Date;
    type: string;   
}

export interface IContent {
    currentFolder: string;
    parent?: string;
    contents: IFileSystem[];
}