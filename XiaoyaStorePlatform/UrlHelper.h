#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Helper
	{
		class UrlHelper
		{
		public:
			static std::string GetHost(const std::string url)
			{
				UriParserStateA state;
				UriUriA uri;

				state.uri = &uri;
				if (uriParseUriA(&state, url.c_str()) != URI_SUCCESS)
				{
					uriFreeUriMembersA(&uri);
					return std::move(std::string());
				}
				auto result = std::string(uri.hostText.first, uri.hostText.afterLast);
				uriFreeUriMembersA(&uri);

				return std::move(result);
			}

			static int GetDomainDepth(const std::string url)
			{
				UriParserStateA state;
				UriUriA uri;

				state.uri = &uri;
				if (uriParseUriA(&state, url.c_str()) != URI_SUCCESS)
				{
					uriFreeUriMembersA(&uri);
					return 0;
				}
				UriPathSegmentStructA * head(uri.pathHead);
				int depth = 0;

				while (head)
				{
					depth++;
					head = head->next;
				}

				return depth;
			}
		};
	}
}