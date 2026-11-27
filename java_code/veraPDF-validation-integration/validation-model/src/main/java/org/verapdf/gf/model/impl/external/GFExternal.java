/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.external;

import org.verapdf.model.GenericModelObject;
import org.verapdf.model.external.External;

/**
 * Base model type for non-COS resources embedded in or referenced by a PDF.
 *
 * <p>External model objects describe parsed resources such as font programs, ICC profiles, CMaps, image codestreams,
 * and signature data. They are produced by higher-level PDF wrappers and expose the common model-object identity
 * while keeping format-specific parsing in the corresponding library.</p>
 *
 * <p><b>.NET porting notes:</b> Add {@code VeraPdf.Validation.Gf.Model.Impl.External.GFExternal}, implementing
 * {@code VeraPdf.Model.External.IExternal} through {@code GenericModelObject}. Prefer typed parser objects from
 * {@code VeraPdf.Parser} where available; do not make this base class own the different resources' lifetimes.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFExternal extends GenericModelObject implements External  {

    protected GFExternal(String type) {
        super(type);
    }

}
