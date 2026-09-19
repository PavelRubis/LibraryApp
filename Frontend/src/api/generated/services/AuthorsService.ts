/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuthorDto } from '../models/AuthorDto';
import type { AuthorDtoPagedResult } from '../models/AuthorDtoPagedResult';
import type { CreateAuthorRequest } from '../models/CreateAuthorRequest';
import type { UpdateAuthorRequest } from '../models/UpdateAuthorRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AuthorsService {
    /**
     * @returns AuthorDto Created
     * @throws ApiError
     */
    public static postApiAuthors({
        requestBody,
    }: {
        requestBody?: CreateAuthorRequest,
    }): CancelablePromise<AuthorDto> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/authors',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns AuthorDtoPagedResult OK
     * @throws ApiError
     */
    public static getApiAuthors({
        q,
        page = 1,
        pageSize = 20,
    }: {
        q?: string,
        page?: number,
        pageSize?: number,
    }): CancelablePromise<AuthorDtoPagedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/authors',
            query: {
                'q': q,
                'page': page,
                'pageSize': pageSize,
            },
        });
    }
    /**
     * @returns AuthorDto OK
     * @throws ApiError
     */
    public static getApiAuthors1({
        id,
    }: {
        id: string,
    }): CancelablePromise<AuthorDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/authors/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns AuthorDto OK
     * @throws ApiError
     */
    public static putApiAuthors({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateAuthorRequest,
    }): CancelablePromise<AuthorDto> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/authors/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns void
     * @throws ApiError
     */
    public static deleteApiAuthors({
        id,
        ifMatch,
    }: {
        id: string,
        ifMatch?: string,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/authors/{id}',
            path: {
                'id': id,
            },
            headers: {
                'If-Match': ifMatch,
            },
        });
    }
}
