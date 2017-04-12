export interface IFileSystem {
    fullName: string;
    name: string;
    size: number;
    lastChanged: Date;
    type: string;   
}

export interface IContent {
    parent?: string;
    contents: IFileSystem[];
}