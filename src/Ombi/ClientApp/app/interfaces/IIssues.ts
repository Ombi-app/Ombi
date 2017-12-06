export interface IIssues {
    id: number;
    subject: string;
    description: string;
    issueCategoryId: number;
    status: IssueStatus;
    resolvedDate: Date;
}

export interface ITvIssues extends IIssues {
    tvId: number;
}

export interface IMovieIssues extends IIssues {
    movieId: number;
}

export enum IssueStatus {
    Pending = 0,
    InProgress = 1,
    Resovled = 2,
}
