// Copyright 2016, 2015, 2014 Matthias Koch
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Requests;
using JetBrains.ReSharper.Feature.Services.Occurences;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Search;

namespace Coconut.DebugNavigation
{
  public class MimicSearchRequest : SearchDeclarationsRequest
  {
    private readonly SearchInheritorsRequest _innerContextSearch;

    public MimicSearchRequest (IDeclaredElement declaredElement)
        : base(declaredElement)
    {
      _innerContextSearch = new SearchInheritorsRequest(declaredElement, declaredElement.GetSearchDomain());
    }

    public override ICollection<IOccurence> Search ()
    {
      return _innerContextSearch.Search();
    }

    public ISearchDomain SearchDomain
    {
      get { return _innerContextSearch.SearchDomain; }
    }

    public override int CompareOccurences (IOccurence x, IOccurence y)
    {
      return _innerContextSearch.CompareOccurences(x, y);
    }

    public override ICollection<IOccurence> Search (IProgressIndicator progressIndicator)
    {
      return _innerContextSearch.Search(progressIndicator);
    }

    public override string Title
    {
      get { return _innerContextSearch.Title; }
    }

    public override ISolution Solution
    {
      get { return _innerContextSearch.Solution; }
    }

    public override ICollection SearchTargets
    {
      get { return _innerContextSearch.SearchTargets; }
    }
  }
}